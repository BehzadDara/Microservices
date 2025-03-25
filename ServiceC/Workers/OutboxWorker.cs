using RabbitMQ.Client;
using System.Text;

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
                    await channel.ExchangeDeclareAsync($"{message.Event}_exchange", ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);
                    await channel.QueueDeclareAsync($"{message.Event}_queue", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
                    await channel.QueueBindAsync($"{message.Event}_queue", $"{message.Event}_exchange", $"{message.Event}_key", cancellationToken: cancellationToken);

                    var body = Encoding.UTF8.GetBytes(message.Body);
                    await channel.BasicPublishAsync($"{message.Event}_exchange", $"{message.Event}_key", body, cancellationToken);

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
