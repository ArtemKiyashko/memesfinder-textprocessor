using Azure;
using Azure.AI.TextAnalytics;
using FluentValidation;
using MemesFinderTextProcessor.Clients;
using MemesFinderTextProcessor.Factories;
using MemesFinderTextProcessor.Interfaces.Adapters;
using MemesFinderTextProcessor.Interfaces.AzureClients;
using MemesFinderTextProcessor.Models;
using Microsoft.Azure.Functions.Worker;
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
        private readonly IValidator<Message> _messageValidator;
        private readonly IValidator<Response<KeyPhraseCollection>> _keyPhraseResponseValidator;
        private readonly IModelAdapter<Message, KeyPhraseCollection> _modelAdapter;

        public MemesFinderTextProcessor(
            ILogger<MemesFinderTextProcessor> log,
            IServiceBusModelSender serviceBusModelSender,
            ITextAnalyticsClient textAnalyticsClient,
            IValidator<Message> messageValidator,
            IValidator<Response<KeyPhraseCollection>> keyPhraseResponseValidator,
            IModelAdapter<Message, KeyPhraseCollection> modelAdapter)
        {
            _logger = log;
            _serviceBusModelSender = serviceBusModelSender;
            _textAnalyticsClient = textAnalyticsClient;
            _messageValidator = messageValidator;
            _keyPhraseResponseValidator = keyPhraseResponseValidator;
            _modelAdapter = modelAdapter;
        }

        [Function("MemesFinderTextProcessor")]
        public async Task Run([ServiceBusTrigger("textmessages", "textprocessor", Connection = "ServiceBusOptions")] Update tgUpdate)
        {
            Message incomeMessage = MessageProcessFactory.GetMessageToProcess(tgUpdate);

            var messageValidationResult = _messageValidator.Validate(incomeMessage);

            if (!messageValidationResult.IsValid)
            {
                _logger.LogInformation(messageValidationResult.ToString());
                return;
            }

            Response<KeyPhraseCollection> response = await _textAnalyticsClient.ExtractKeyPhrasesAsync(incomeMessage.Text);

            var keyPhraseValidationResult = _keyPhraseResponseValidator.Validate(response);

            if (!keyPhraseValidationResult.IsValid)
            {
                _logger.LogInformation(keyPhraseValidationResult.ToString());
                return;
            }

            TgMessageModel tgMessageModel = _modelAdapter.Adapt(incomeMessage, response.Value);

            await _serviceBusModelSender.SendMessageAsync(tgMessageModel);
        }
    }
}

