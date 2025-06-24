using System.ComponentModel.DataAnnotations.Schema;
using CrmSystem.Models;

public class Deal
{
    public int Id { get; set; }
    public string Name { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    public DealStage Stage { get; set; }
    public string? Resources { get; set; }
    public string? Duration { get; set; } 
    public DateTime? ExpectedCloseDate { get; set; }
    public int? ClientId { get; set; }
    public Client? Client { get; set; }
    public int? OwnerId { get; set; }
    public User? Owner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public string? LostReason { get; set; }
    public string? Notes { get; set; }
}
