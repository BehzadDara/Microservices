using System.ComponentModel.DataAnnotations;

namespace ServiceA.Models;

public class ModelA1
{
    [Key] public int Id { get; set; }
    public required string Title { get; set; }
    public string? ShortCode { get; set; }
}
