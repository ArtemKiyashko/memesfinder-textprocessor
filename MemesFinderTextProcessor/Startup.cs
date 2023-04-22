using Azure.AI.TextAnalytics;
using FluentValidation;
using MemesFinderTextProcessor.Adapters;
using MemesFinderTextProcessor.Extensions;
using MemesFinderTextProcessor.Interfaces.Adapters;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

[assembly: FunctionsStartup(typeof(MemesFinderTextProcessor.Startup))]
namespace MemesFinderTextProcessor
{
    public class Startup : FunctionsStartup
    {
        private IConfigurationRoot _functionConfig;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            _functionConfig = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddServiceBusKeywordClient(_functionConfig);
            builder.Services.AddTextAnalyticsClient(_functionConfig);
            builder.Services.AddScoped<IModelAdapter<Message, KeyPhraseCollection>, TgMessageToModelAdapter>();

            builder.Services.AddValidatorsFromAssemblyContaining<Startup>();

            builder.Services.AddLogging(logBuilder =>
            {
                logBuilder.AddConsole();
            });
        }
    }
}

