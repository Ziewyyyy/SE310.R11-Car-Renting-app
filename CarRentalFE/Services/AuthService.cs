
using System.Net.Http.Json;
using CarRentalFE.Models;

namespace CarRentalFE.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "auth/register",
            new
            {
                request.FullName,
                request.Email,
                request.Phone,
                request.Password
            });

        return response.IsSuccessStatusCode;
    }
}
