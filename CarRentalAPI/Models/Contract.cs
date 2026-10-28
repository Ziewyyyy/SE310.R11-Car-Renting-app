namespace CarRentalAPI.Models;

public class Contract
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public DateTime SignedAt { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DepositAmount { get; set; }
    public string Status { get; set; } = "Pending";
}
