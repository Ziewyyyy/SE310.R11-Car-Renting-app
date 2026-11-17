namespace CarRentalFE.Models.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = new();
}