using Azure.AI.TextAnalytics;
using Azure.Identity;
using MemesFinderTextProcessor.Clients;
using MemesFinderTextProcessor.Clients.AzureClients;
using MemesFinderTextProcessor.Interfaces.AzureClients;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MemesFinderTextProcessor.Extensions
{
    public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddServiceBusKeywordClient(this IServiceCollection services, IConfiguration configuration)
		{
            services.Configure<ServiceBusOptions>(configuration.GetSection("ServiceBusOptions"));
            var fullyQualifiedNamespace = configuration["ServiceBusOptions:FullyQualifiedNamespace"]
                ?? throw new InvalidOperationException("ServiceBusOptions:FullyQualifiedNamespace is not configured.");

            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.UseCredential(new DefaultAzureCredential());
                clientBuilder.AddServiceBusClientWithNamespace(fullyQualifiedNamespace);
            });

            services.AddTransient<IServiceBusClient, ServiceBusKeywordMessagesClient>();
            services.AddTransient<IServiceBusModelSender, ServiceBusModelSender>();
            return services;
		}

        public static IServiceCollection AddTextAnalyticsClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TextAnalyticsOptions>(configuration.GetSection("TextAnalyticsOptions"));
            var options = configuration.GetSection("TextAnalyticsOptions").Get<TextAnalyticsOptions>()
                ?? throw new InvalidOperationException("TextAnalyticsOptions are not configured.");

            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder
                    .AddTextAnalyticsClient(options.Url)
                    .ConfigureOptions(clientOptions => clientOptions.DefaultLanguage = options.Language);
            });

            services.AddTransient<ITextAnalyticsClient, KeyPhraseExtractor>();

            return services;
        }

    }
}

