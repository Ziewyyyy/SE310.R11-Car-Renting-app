namespace CarRentalAPI.Models;

public class RentalExtension
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public DateTime NewReturnDate { get; set; }
    public decimal ExtraAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
