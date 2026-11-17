using System.Net.Http.Json;
using CarRentalFE.Models.Auth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CarRentalFE.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ProtectedLocalStorage _storage;

    public AuthService(HttpClient httpClient, ProtectedLocalStorage storage)
    {
        _httpClient = httpClient;
        _storage = storage;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/register", request);
            return response.IsSuccessStatusCode
                ? new AuthResult(true, "Đăng ký thành công.")
                : new AuthResult(false, await ReadErrorAsync(response));
        }
        catch (HttpRequestException)
        {
            return new AuthResult(false, "Không kết nối được máy chủ.");
        }
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", request);
            if (!response.IsSuccessStatusCode)
                return new AuthResult(false, await ReadErrorAsync(response));

            var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (data is null)
                return new AuthResult(false, "Phản hồi từ máy chủ không hợp lệ.");

            await _storage.SetAsync("auth", data);
            return new AuthResult(true);
        }
        catch (HttpRequestException)
        {
            return new AuthResult(false, "Không kết nối được máy chủ.");
        }
    }

    public async Task LogoutAsync()
    {
        await _storage.DeleteAsync("auth");
    }

    private sealed record ErrorBody(string? Message);

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var raw = await response.Content.ReadAsStringAsync();
        try
        {
            var body = System.Text.Json.JsonSerializer.Deserialize<ErrorBody>(
                raw, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
            if (!string.IsNullOrWhiteSpace(body?.Message))
                return body.Message;
        }
        catch { }

        return $"Lỗi {(int)response.StatusCode}: {raw}";
    }

}