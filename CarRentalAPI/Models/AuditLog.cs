namespace CarRentalAPI.Models;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ActorId { get; set; }
    public string Action { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string EntityId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
