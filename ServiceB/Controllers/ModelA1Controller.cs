using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceB.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace ServiceB.Controllers;

[Route("ModelA1")]
public class ModelA1Controller(ServiceBDBContext context, IConnectionMultiplexer redis) : ControllerBase
{
    private readonly DbSet<ModelA1> set = context.Set<ModelA1>();
    private readonly IDatabase cache = redis.GetDatabase();
    private readonly string cacheKey = "modelA1_key";

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var cachedData = await cache.StringGetAsync(cacheKey);
        if (cachedData.HasValue)
        {
            var cachedModelA1s = JsonSerializer.Deserialize<List<ModelA1>>(cachedData!);
            return Ok(cachedModelA1s);
        }

        var modelA1s = await set.ToListAsync(cancellationToken);

        var expiration = TimeSpan.FromMinutes(10);
        await cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(modelA1s), expiration);

        return Ok(modelA1s);
    }
}
