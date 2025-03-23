using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServiceC.Models;
using ServiceC.Publishers;
using ServiceC.Services;
using System.Text;
using System.Text.Json;

namespace ServiceC.Consumers;

public class ModelA1Consumer : BackgroundService
{
    private readonly ShortCodeService _service;
    private readonly ModelA1ShortCodePublisher _publisher;
    private readonly IChannel _channel;

    private const string ExchangeName = "modelA1_exchange";
    private const string QueueName = "modelA1_queue2";

    public ModelA1Consumer(ShortCodeService service, ModelA1ShortCodePublisher publisher, IChannel channel)
    {
        _service = service;
        _publisher = publisher;
        _channel = channel;

        _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);
        _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBindAsync(QueueName, ExchangeName, string.Empty);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var request = JsonSerializer.Deserialize<ModelA1>(message);
                if (request != null)
                {
                    var shortCode = _service.Generate(request.Title);
                    request.ShortCode = shortCode;

                    await _publisher.PublishMessageAsync(request, cancellationToken);
                }

                await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken);
            }
            catch (Exception)
            {
                await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, cancellationToken);
            }
        };

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, cancellationToken);
    }
}
