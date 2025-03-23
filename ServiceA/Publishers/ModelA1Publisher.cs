using RabbitMQ.Client;
using ServiceA.Models;
using System.Text;
using System.Text.Json;

namespace ServiceA.Publishers
{
    public class ModelA1Publisher
    {
        private readonly IChannel _channel;

        private const string ExchangeName = "modelA1_exchange";
        private const string QueueName = "modelA1_queue";
        private const string RoutingKey = "modelA1_key";

        public ModelA1Publisher(IChannel channel)
        {
            _channel = channel;

            _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
            _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey);
        }

        public async Task PublishMessageAsync(ModelA1 modelA1, CancellationToken cancellationToken)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(modelA1));
            await _channel.BasicPublishAsync(ExchangeName, RoutingKey, body, cancellationToken);
        }
    }
}
