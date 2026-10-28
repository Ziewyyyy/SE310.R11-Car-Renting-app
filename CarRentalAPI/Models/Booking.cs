namespace CarRentalAPI.Models;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public Guid CarId { get; set; }
    public Car Car { get; set; } = null!;

    public DateTime PickupDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;
}