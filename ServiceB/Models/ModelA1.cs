using System.ComponentModel.DataAnnotations;

namespace ServiceB.Models;

public class ModelA1
{
    [Key] public int Id { get; set; }
    public required string Title { get; set; }
}
