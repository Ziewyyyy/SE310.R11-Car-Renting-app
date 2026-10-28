namespace CarRentalAPI.Models;

public class BookingDetail
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Guid CarId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}
