using Azure.AI.TextAnalytics;
using Azure.Monitor.OpenTelemetry.Exporter;
using FluentValidation;
using MemesFinderTextProcessor.Adapters;
using MemesFinderTextProcessor.Extensions;
using MemesFinderTextProcessor.Interfaces.Adapters;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using Azure.Core.Serialization;
using Telegram.Bot.Types;

var builder = FunctionsApplication.CreateBuilder(args);

AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource("Azure.Messaging.ServiceBus.*"))
    .UseFunctionsWorkerDefaults()
    .UseAzureMonitorExporter();

builder.Services.Configure<WorkerOptions>(options =>
    options.Serializer = new NewtonsoftJsonObjectSerializer());

builder.Services.AddServiceBusKeywordClient(builder.Configuration);
builder.Services.AddTextAnalyticsClient(builder.Configuration);
builder.Services.AddScoped<IModelAdapter<Message, KeyPhraseCollection>, TgMessageToModelAdapter>();
builder.Services.AddValidatorsFromAssemblyContaining<global::MemesFinderTextProcessor.MemesFinderTextProcessor>();

builder.Build().Run();
