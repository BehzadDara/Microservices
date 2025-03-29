using RabbitMQ.Client;
using ServiceC.Models;
using System.Text;
using System.Text.Json;

namespace ServiceC.Workers;

public class OutboxWorker(IServiceProvider serviceProvider, IChannel channel) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ServiceCDBContext>();

            var messages = context.OutboxMessages
                .Where(x => !x.IsPublished)
                .ToList();

            foreach (var message in messages)
            {
                try
                {
                    await channel.ExchangeDeclareAsync(
                        exchange: $"{message.Event}_exchange",
                        type: "x-delayed-message",
                        durable: true,
                        autoDelete: false,
                        arguments: new Dictionary<string, object?>
                        {
                            { "x-delayed-type", "direct" }
                        },
                        cancellationToken: cancellationToken);

                    await channel.QueueDeclareAsync(
                        queue: $"{message.Event}_queue",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: cancellationToken);

                    await channel.QueueBindAsync(
                        queue: $"{message.Event}_queue",
                        exchange: $"{message.Event}_exchange",
                        routingKey: $"{message.Event}_key",
                        cancellationToken: cancellationToken);

                    var body = Encoding.UTF8.GetBytes(message.Body);

                    var modelA1 = JsonSerializer.Deserialize<ModelA1>(message.Body)!;
                    var IsImmediate = modelA1.Id % 2 == 0;
                    var delayInMiliseconds = IsImmediate ? 0: 5000;

                    var properties = new BasicProperties
                    {
                        Persistent = true,
                        Headers = new Dictionary<string, object?>
                        {
                            { "x-delay", delayInMiliseconds }
                        }
                    };

                    await channel.BasicPublishAsync(
                        exchange: $"{message.Event}_exchange",
                        routingKey: $"{message.Event}_key",
                        body: body,
                        basicProperties: properties,
                        mandatory: false,
                        cancellationToken: cancellationToken);

                    MetricsService.RabbitMessagesSent.Inc();

                    message.IsPublished = true;
                    message.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception)
                {
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }
}