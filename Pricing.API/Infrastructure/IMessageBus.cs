using Pricing.API.Interfaces;

namespace Pricing.API.Infrastructure
{
    public interface IMessageBus
    {
        Task InitializeAsync();
        Task PublishAsync<T>(T message);
        Task SubscribeAsync<TRequest, THandler>(string queueName, string exchange) where TRequest : IRequest where THandler : IRequestHandler<TRequest>;
    }
}
