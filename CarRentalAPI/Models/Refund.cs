namespace CarRentalAPI.Models;

public class Refund
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public string Reason { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
