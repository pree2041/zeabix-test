using RabbitMQ.Client;

namespace Pricing.API.Infrastructure
{
    public class RabbitMqConnectionService : IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public RabbitMqConnectionService(IConfiguration configuration)
        {
            _factory = new ConnectionFactory
            {
                HostName = configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost",
                Port = configuration.GetValue<int>("RabbitMQ:Port", 5672),
                UserName = configuration.GetValue<string>("RabbitMQ:UserName") ?? "guest",
                Password = configuration.GetValue<string>("RabbitMQ:Password") ?? "guest"
            };
        }

        private async Task<IConnection> GetConnectionAsync()
        {
            if (_connection is { IsOpen: true })
                return _connection;

            await _semaphore.WaitAsync();
            try
            {
                if (_connection is { IsOpen: true })
                    return _connection;

                _connection = await _factory.CreateConnectionAsync();
                return _connection;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IChannel> CreateChannelAsync()
        {
            var connection = await GetConnectionAsync(); 
            return await connection.CreateChannelAsync(); 
        }
        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();
        }
    }

}
