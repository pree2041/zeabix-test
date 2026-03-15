using RabbitMQ.Client;
using Pricing.API.Handlers;
using Pricing.API.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Pricing.API.Infrastructure
{
    public static class MessagingRegistration
    {
        public static IServiceCollection AddMessagingInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<RabbitMqConnectionService>();
            services.AddSingleton<IMessageBus, RabbitMqBus>();
            services.AddScoped<SubmitBulkQuotesCommandHandler>();

            return services;
        }

        public static async Task RegisterSubscriptions(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var exchange = configuration["RabbitMQ:DefaultExchange"] ?? "pricing-exchange";
            var queue = configuration["RabbitMQ:DefaultQueue"] ?? "pricing-queue";
            var bus = serviceProvider.GetRequiredService<IMessageBus>();
            await bus.InitializeAsync();
            await bus.SubscribeAsync<SubmitBulkQuotesCommand, SubmitBulkQuotesCommandHandler>(queue, exchange);
        }

        public static async Task RegisterSubscriptionsWithRetry(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<RabbitMqConnectionService>>();
            int retries = 5;
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    await RegisterSubscriptions(serviceProvider, configuration);
                    logger.LogInformation("✅ Successfully subscribed to RabbitMQ.");
                    return;
                }
                catch (Exception ex)
                {
                    if (i == retries - 1) logger.LogError(ex, "❌ Critical: Could not connect to RabbitMQ after multiple attempts.");
                    else
                    {
                        logger.LogWarning($"⚠️ RabbitMQ not ready. Retrying in 2s... (Attempt {i + 1}/{retries})");
                        await Task.Delay(2000);
                    }
                }
            }
        }
    }
}
