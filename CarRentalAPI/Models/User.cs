namespace CarRentalAPI.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Role { get; set; } = "Customer";

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}