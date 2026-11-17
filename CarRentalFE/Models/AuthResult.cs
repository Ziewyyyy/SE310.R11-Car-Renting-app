namespace CarRentalFE.Models.Auth;

public record AuthResult(bool Success, string Message = "", string Role = "");
