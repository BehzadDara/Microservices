﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServiceA.Models;
using System.Text;
using System.Text.Json;

namespace ServiceA.Consumers;

public class ModelA1ShortCodeConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IChannel _channel;

    private const string ExchangeName = "modelA1ShortCode_exchange";
    private const string QueueName = "modelAShortCode1_queue";
    private const string RoutingKey = "modelA1ShortCode_key";

    public ModelA1ShortCodeConsumer(IServiceScopeFactory serviceScopeFactory, IChannel channel)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _channel = channel;

        _channel.ExchangeDeclareAsync(
            exchange: ExchangeName, 
            type: "x-delayed-message", 
            durable: true, 
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                { "x-delayed-type", "direct" }
            });

        _channel.QueueDeclareAsync(
            queue: QueueName, 
            durable: true, 
            exclusive: false, 
            autoDelete: false, 
            arguments: null);

        _channel.QueueBindAsync(
            queue: QueueName, 
            exchange: ExchangeName, 
            routingKey: RoutingKey);
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

                MetricsService.RabbitMessagesReceived.Inc();

                var request = JsonSerializer.Deserialize<ModelA1>(message);
                if (request != null)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ServiceADBContext>();
                    var set = context.Set<ModelA1>();

                    var modelA1 = await set.FindAsync([request.Id], cancellationToken);
                    if (modelA1 is not null)
                    {
                        modelA1.ShortCode = request.ShortCode;
                        await context.SaveChangesAsync(cancellationToken);
                    }
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
