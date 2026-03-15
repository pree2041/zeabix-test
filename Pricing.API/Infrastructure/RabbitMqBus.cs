
using Microsoft.Extensions.Configuration;
using Pricing.API.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Pricing.API.Infrastructure
{
    public class RabbitMqBus : IMessageBus
    {
        private readonly RabbitMqConnectionService _connectionService;
        private IChannel? _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _mainEx = string.Empty;
        private readonly string _mainQ = string.Empty;
        private readonly string _retryEx = string.Empty;
        private readonly string _retryQ = string.Empty;
        private readonly string _dlxEx = string.Empty;
        private readonly string _dlxQ = string.Empty;
        private readonly int _ttl;
        private readonly int _maxRetry;
        public RabbitMqBus(RabbitMqConnectionService connectionService, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _connectionService = connectionService;
            _serviceProvider = serviceProvider;
            var config = configuration.GetSection("RabbitMQ");
            _mainEx = config["DefaultExchange"] ?? "pricing-exchange";
            _mainQ = config["DefaultQueue"] ?? "pricing-queue";
            _retryEx = config["RetryExchangeName"] ?? "pricing-retry-exchange";
            _retryQ = config["RetryQueueName"] ?? "pricing-retry-queue";
            _dlxEx = config["DeadLetterExchange"] ?? "dlx-exchange";
            _dlxQ = config["DeadLetterQueue"] ?? "dlx-queue";
            _ttl = config.GetValue<int>("TimeToLive");
            _maxRetry = config.GetValue<int>("MaxRetry");
        }

        public async Task InitializeAsync()
        {
            _channel = await _connectionService.CreateChannelAsync();
            var mainQArgs = new Dictionary<string, object?> {
                { "x-dead-letter-exchange", _retryEx }
            };
            var retryQArgs = new Dictionary<string, object?> {
                { "x-dead-letter-exchange", _mainEx },
                { "x-message-ttl", _ttl }
            };

            await _channel.ExchangeDeclareAsync(_mainEx, ExchangeType.Direct, durable: true);
            await _channel.ExchangeDeclareAsync(_retryEx, ExchangeType.Direct, durable: true);
            await _channel.ExchangeDeclareAsync(_dlxEx, ExchangeType.Fanout, durable: true);

            await _channel.QueueDeclareAsync(_mainQ, durable: true, false, false, mainQArgs);
            await _channel.QueueDeclareAsync(_retryQ, durable: true, false, false, retryQArgs);
            await _channel.QueueDeclareAsync(_dlxQ, durable: true, false, false);
            await _channel.QueueBindAsync(_dlxQ, _dlxEx, routingKey: "");

        }


        public async Task SubscribeAsync<TRequest, THandler>(string queueName, string exchange) where TRequest : IRequest where THandler : IRequestHandler<TRequest>
        {
            if (_channel == null || !_channel.IsOpen)
            {
                _channel = await _connectionService.CreateChannelAsync();
            }
            string routingKey = typeof(TRequest).Name;

            await _channel.QueueBindAsync(queueName, exchange, routingKey);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                int retryCount = GetRetryCount(ea.BasicProperties.Headers);
                var body = ea.Body.ToArray();

                try
                {
                    var request = JsonSerializer.Deserialize<TRequest>(Encoding.UTF8.GetString(body));
                    if (request != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var handler = scope.ServiceProvider.GetRequiredService<THandler>();

                        await handler.HandleAsync(request);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    if (retryCount >= _maxRetry)
                    {
                        await _channel.BasicPublishAsync(exchange: _dlxEx, routingKey: routingKey, body: body);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    else
                    {
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    }
                }
            };

            await _channel.BasicConsumeAsync(queueName, false, consumer);
        }

        private int GetRetryCount(IDictionary<string, object?>? headers)
        {
            if (headers == null || !headers.ContainsKey("x-death")) return 0;

            var deathList = headers["x-death"] as IList<object>;
            if (deathList == null || deathList.Count == 0) return 0;

            var deathEntry = deathList[0] as IDictionary<string, object>;
            if (deathEntry != null && deathEntry.ContainsKey("count"))
            {
                return Convert.ToInt32(deathEntry["count"]);
            }

            return deathList.Count;
        }


        public async Task PublishAsync<T>(T message)
        {
            if (_channel == null || !_channel.IsOpen)
            {
                _channel = await _connectionService.CreateChannelAsync();
            }
            string routingKey = typeof(T).Name;

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            var properties = new BasicProperties
            {
                Persistent = true,
                ContentEncoding = "utf-8",
                ContentType = "application/json",
                Headers = new Dictionary<string, object?>()
            };

            await _channel.BasicPublishAsync(
                exchange: _mainEx,
                mandatory: true,
                routingKey: routingKey,
                basicProperties: properties,
                body: body
            );

        }


    }
}
