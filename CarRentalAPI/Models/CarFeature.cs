namespace CarRentalAPI.Models;

public class CarFeature
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}
