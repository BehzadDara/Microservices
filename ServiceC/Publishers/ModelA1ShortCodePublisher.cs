using RabbitMQ.Client;
using ServiceC.Models;
using System.Text;
using System.Text.Json;

namespace ServiceC.Publishers
{
    public class ModelA1ShortCodePublisher
    {
        private readonly IChannel _channel;

        private const string ExchangeName = "modelA1ShortCode_exchange";
        private const string QueueName = "modelA1ShortCode_queue";
        private const string RoutingKey = "modelA1ShortCode_key";

        public ModelA1ShortCodePublisher(IChannel channel)
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
