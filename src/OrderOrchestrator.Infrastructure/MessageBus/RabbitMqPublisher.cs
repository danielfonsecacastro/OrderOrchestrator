using Microsoft.Extensions.Options;
using OrderOrchestrator.Domain.Interfaces;
using OrderOrchestrator.Infrastructure.Configurations;
using RabbitMQ.Client;
using System.Text;

namespace OrderOrchestrator.Infrastructure.MessageBus
{
    public class RabbitMqPublisher : IMessageBus
    {
        private readonly ConnectionFactory _factory;
        private readonly RabbitMqOptions _rabbitMqOptions;

        public RabbitMqPublisher(IOptions<RabbitMqOptions> opts)
        {
            _rabbitMqOptions = opts.Value;
            _factory = new ConnectionFactory() { HostName = _rabbitMqOptions.HostName };
        }

        public async Task Publish(string queue, string message)
        {
            using (var connection = await _factory.CreateConnectionAsync())
            using (var channel = await connection.CreateChannelAsync())
            {
                await channel.QueueDeclareAsync(queue: queue,
                                     durable: _rabbitMqOptions.Durable,
                                     exclusive: _rabbitMqOptions.Exclusive,
                                     autoDelete: _rabbitMqOptions.AutoDelete,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);
                await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue, body: body, mandatory: true);
            }
        }
    }
}
