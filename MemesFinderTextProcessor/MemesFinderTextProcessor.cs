﻿using System.Threading.Tasks;
using MemesFinderTextProcessor.Interfaces.AzureClients;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace MemesFinderTextProcessor
{
    public class MemesFinderTextProcessor
    {
        private readonly ILogger<MemesFinderTextProcessor> _logger;
        private readonly IServiceBusClient _serviceBusClient;
        private readonly ITextAnalyticsClient _textAnalyticsClient;

        public MemesFinderTextProcessor(
            ILogger<MemesFinderTextProcessor> log,
            IServiceBusClient serviceBusClient,
            ITextAnalyticsClient textAnalyticsClient)
        {
            _logger = log;
            _serviceBusClient = serviceBusClient;
            _textAnalyticsClient = textAnalyticsClient;
        }

        [FunctionName("MemesFinderTextProcessor")]
        public async Task Run([ServiceBusTrigger("allmessages", "textprocessor", Connection = "ServiceBusOptions")] Update tgUpdate)
        {

        }
    }
}

