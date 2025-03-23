using Microsoft.EntityFrameworkCore;
using ServiceA.Models;

namespace ServiceA;

public class ServiceADBContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ModelA1> ModelA1s { get; set; }
}
