namespace CarRentalAPI.Models;

public class TestDriveBooking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Guid CarId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = "Pending";
}
