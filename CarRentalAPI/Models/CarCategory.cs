namespace CarRentalAPI.Models;

public class CarCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    public ICollection<Car> Cars { get; set; }
        = new List<Car>();
}