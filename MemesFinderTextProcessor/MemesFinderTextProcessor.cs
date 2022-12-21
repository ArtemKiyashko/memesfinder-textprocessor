﻿using Azure;
using Azure.AI.TextAnalytics;
using MemesFinderTextProcessor.Clients;
using MemesFinderTextProcessor.Factories;
using MemesFinderTextProcessor.Interfaces.AzureClients;
using MemesFinderTextProcessor.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MemesFinderTextProcessor
{
    public class MemesFinderTextProcessor
    {
        private readonly ILogger<MemesFinderTextProcessor> _logger;
        private readonly IServiceBusModelSender _serviceBusModelSender;
        private readonly ITextAnalyticsClient _textAnalyticsClient;

        public MemesFinderTextProcessor(
            ILogger<MemesFinderTextProcessor> log,
            IServiceBusModelSender serviceBusModelSender,
            ITextAnalyticsClient textAnalyticsClient)
        {
            _logger = log;
            _serviceBusModelSender = serviceBusModelSender;
            _textAnalyticsClient = textAnalyticsClient;
        }

        [FunctionName("MemesFinderTextProcessor")]
        public async Task Run([ServiceBusTrigger("allmessages", "textprocessor", Connection = "ServiceBusOptions")] Update tgUpdate)
        {
            Message incomeMessage = new MessageProcessFactory().GetMessageToProcess(tgUpdate);
            if (string.IsNullOrEmpty(incomeMessage.Text))
            {
                _logger.LogInformation("Message not a text");
                return;
            }

            Response<KeyPhraseCollection> response = await _textAnalyticsClient.ExtractKeyPhrasesAsync(incomeMessage.Text);

            if (response.Value.Count == 0)
            {
                _logger.LogInformation("Key phrases not found");
                return;
            }

            KeyPhraseCollection keyPhrases = response.Value;

            var tgMessageModel = new TgMessageModel
            {
                Message = incomeMessage,
                //return random array element from keyPhrases
                Keyword = keyPhrases[new Random().Next(0, keyPhrases.Count)]
            };

            await _serviceBusModelSender.SendMessageAsync(tgMessageModel);
        }
    }
}

