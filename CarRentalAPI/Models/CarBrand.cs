namespace CarRentalAPI.Models;

public class CarBrand
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";

    public ICollection<Car> Cars { get; set; }
        = new List<Car>();
}