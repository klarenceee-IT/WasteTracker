using System.ComponentModel.DataAnnotations;

namespace WasteTracker.Api.Models;

public class WasteLogEntity
{
    [Key]
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public double Kg { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}