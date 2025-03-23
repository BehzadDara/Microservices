using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceB.Models;

namespace ServiceB.Controllers;

[Route("ModelA1")]
public class ModelA1Controller(ServiceBDBContext context) : ControllerBase
{
    private readonly DbSet<ModelA1> set = context.Set<ModelA1>();

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var modelA1s = await set.ToListAsync(cancellationToken);
        return Ok(modelA1s);
    }
}
