using ServiceC.Models;
using System.Text.Json;

namespace ServiceC.Publishers;

public class ModelA1ShortCodePublisher(IServiceScopeFactory serviceScopeFactory)
{
    public async Task PublishMessageAsync(ModelA1 modelA1, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ServiceCDBContext>();

        var outboxMessage = new OutboxMessage
        {
            Event = "modelA1ShortCode",
            Body = JsonSerializer.Serialize(modelA1)
        };

        await context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
