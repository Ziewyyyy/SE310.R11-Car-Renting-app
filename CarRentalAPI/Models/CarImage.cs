namespace CarRentalAPI.Models;

public class CarImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarId { get; set; }
    public string Url { get; set; } = "";
    public bool IsPrimary { get; set; } = false;
}
