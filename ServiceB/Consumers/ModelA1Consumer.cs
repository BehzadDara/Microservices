using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServiceB.Models;
using System.Text;
using System.Text.Json;

namespace ServiceB.Consumers;

public class ModelA1Consumer : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IChannel _channel;

    private const string ExchangeName = "modelA1_exchange";
    private const string QueueName = "modelA1_queue";
    private const string RoutingKey = "modelA1_key";

    public ModelA1Consumer(IServiceScopeFactory serviceScopeFactory, IChannel channel)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _channel = channel;

        _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
        _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey);
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
                    var context = scope.ServiceProvider.GetRequiredService<ServiceBDBContext>();
                    var set = context.Set<ModelA1>();

                    var modelA1 = await set.FindAsync([request.Id], cancellationToken);
                    if (modelA1 is null)
                    {
                        await set.AddAsync(request, cancellationToken);
                    }
                    else
                    {
                        modelA1.Title = request.Title;
                    }

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
