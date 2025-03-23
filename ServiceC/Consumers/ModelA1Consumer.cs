using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServiceC.Models;
using ServiceC.Services;
using System.Text;
using System.Text.Json;

namespace ServiceC.Consumers;

public class ModelA1Consumer : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ShortCodeService _service;
    private readonly IChannel _channel;

    private const string ExchangeName = "modelA1_exchange";
    private const string QueueName = "modelA1_queue2";

    public ModelA1Consumer(IServiceScopeFactory serviceScopeFactory, ShortCodeService service, IChannel channel)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _service = service;
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
                    using var scope = _serviceScopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ServiceCDBContext>();

                    var shortCode = _service.Generate(request.Title);
                    request.ShortCode = shortCode;

                    var outboxMessage = new OutboxMessage
                    {
                        Event = "modelA1ShortCode",
                        Body = JsonSerializer.Serialize(request)
                    };

                    await context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
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
