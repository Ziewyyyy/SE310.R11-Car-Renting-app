namespace CarRentalAPI.Models;

public class CarFeatureMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarId { get; set; }
    public Guid CarFeatureId { get; set; }
}
