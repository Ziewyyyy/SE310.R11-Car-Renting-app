namespace CarRentalAPI.Models;

public class SupportRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Guid? StaffId { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
