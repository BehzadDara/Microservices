using Microsoft.EntityFrameworkCore;
using ServiceC.Models;

namespace ServiceC;

public class ServiceCDBContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
}
