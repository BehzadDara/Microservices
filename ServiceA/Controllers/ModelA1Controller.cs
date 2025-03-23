using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceA.DTOs;
using ServiceA.Models;
using ServiceA.Publishers;

namespace ServiceA.Controllers;

[Route("ModelA1")]
public class ModelA1Controller(ServiceADBContext context, ModelA1Publisher publisher) : ControllerBase
{
    private readonly DbSet<ModelA1> set = context.Set<ModelA1>();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ModelA1CreateOrUpdateDTO request, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var modelA1 = new ModelA1 { Title = request.Title }; 

        await set.AddAsync(modelA1, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await publisher.PublishMessageAsync(modelA1, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Ok(modelA1);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ModelA1CreateOrUpdateDTO request, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var modelA1 = await set.FindAsync([id], cancellationToken);
        if (modelA1 == null)
        {
            return NotFound();
        }

        modelA1.Title = request.Title;

        await context.SaveChangesAsync(cancellationToken);

        await publisher.PublishMessageAsync(modelA1, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Ok();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var modelA1 = await set.FindAsync([id], cancellationToken);
        if (modelA1 == null)
        {
            return NotFound();
        }
        return Ok(modelA1);
    }
}
