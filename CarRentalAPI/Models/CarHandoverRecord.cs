namespace CarRentalAPI.Models;

public class CarHandoverRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Guid? StaffId { get; set; }
    public string RecordType { get; set; } = "Pickup";
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public int Mileage { get; set; }
    public string ConditionNotes { get; set; } = "";
}
