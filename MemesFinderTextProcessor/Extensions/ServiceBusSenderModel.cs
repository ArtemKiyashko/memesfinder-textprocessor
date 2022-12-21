﻿using MemesFinderTextProcessor.Interfaces.AzureClients;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;
using MemesFinderTextProcessor.Models;

namespace MemesFinderTextProcessor.Extensions
{
    //serialise the object and send it to the server 
    public class ServiceBusSenderModel
    {
        private readonly ILogger<MemesFinderTextProcessor> _logger;
        private readonly IServiceBusClient _serviceBusClient;

        public ServiceBusSenderModel(ILogger<MemesFinderTextProcessor> log, IServiceBusClient serviceBusClient)
        {
            _logger = log;
            _serviceBusClient = serviceBusClient;
        }

        public async Task SendMessageAsync(TgMessageModel tgMessageModel)
        {
            try
            {
                await using ServiceBusSender sender = _serviceBusClient.CreateSender();
                ServiceBusMessage serviceBusMessage = new(tgMessageModel.ToJson());
                await sender.SendMessageAsync(serviceBusMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending message to Service Bus");
            }
        }

    }
        

}