namespace CarRentalAPI.Models;

public class Car
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "";
    public string LicensePlate { get; set; } = "";
    public int ManufactureYear { get; set; }
    public decimal PricePerDay { get; set; }
    public string Color { get; set; } = "";
    public int Seats { get; set; }
    public int Mileage { get; set; }

    public string Status { get; set; } = "Available";

    public Guid CategoryId { get; set; }
    public CarCategory Category { get; set; } = null!;

    public Guid BrandId { get; set; }
    public CarBrand Brand { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}