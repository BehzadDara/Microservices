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
        private const string QueueName1 = "modelA1_queue1";
        private const string QueueName2 = "modelA1_queue2";

        public ModelA1Publisher(IChannel channel)
        {
            _channel = channel;

            _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);

            _channel.QueueDeclareAsync(QueueName1, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclareAsync(QueueName2, durable: true, exclusive: false, autoDelete: false, arguments: null);

            _channel.QueueBindAsync(QueueName1, ExchangeName, string.Empty);
            _channel.QueueBindAsync(QueueName2, ExchangeName, string.Empty);
        }

        public async Task PublishMessageAsync(ModelA1 modelA1, CancellationToken cancellationToken)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(modelA1));
            await _channel.BasicPublishAsync(ExchangeName, string.Empty, body, cancellationToken);

            MetricsService.RabbitMessagesSent.Inc();
        }
    }
}
