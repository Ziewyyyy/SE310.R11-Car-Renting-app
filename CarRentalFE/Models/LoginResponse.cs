namespace CarRentalFE.Models.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public DateTime AccessTokenExpiresAt { get; set; }
    public UserInfo User { get; set; } = new();
}