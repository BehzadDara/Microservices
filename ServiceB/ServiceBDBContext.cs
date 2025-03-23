using Microsoft.EntityFrameworkCore;
using ServiceB.Models;

namespace ServiceB;

public class ServiceBDBContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ModelA1> ModelA1s { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ModelA1>()
            .Property(m => m.Id)
            .ValueGeneratedNever();
    }
}
