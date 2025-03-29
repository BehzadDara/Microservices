using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceB.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace ServiceB.Controllers;

[Route("ServiceB/ModelA1")]
public class ModelA1Controller(
    ServiceBDBContext context, 
    IConnectionMultiplexer redis,
    IConfiguration configuration
    ) : ControllerBase
{
    private readonly DbSet<ModelA1> set = context.Set<ModelA1>();
    private readonly IDatabase cache = redis.GetDatabase();
    private readonly string cacheKey = "modelA1_key";

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var source = configuration.GetValue<string>("Service:ID");

        var cachedData = await cache.StringGetAsync(cacheKey);
        if (cachedData.HasValue)
        {
            var cachedModelA1s = JsonSerializer.Deserialize<List<ModelA1>>(cachedData!);
            return Ok(new
            {
                source,
                cachedModelA1s
            });
        }

        var modelA1s = await set.ToListAsync(cancellationToken);

        var expiration = TimeSpan.FromMinutes(10);
        await cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(modelA1s), expiration);

        return Ok(new
        {
            source,
            modelA1s
        });
    }
}
