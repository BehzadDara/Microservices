using System.ComponentModel.DataAnnotations;

namespace ServiceC.Models;

public class OutboxMessage
{
    [Key] public int Id { get; set; }
    public required string Event { get; set; }
    public required string Body { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
