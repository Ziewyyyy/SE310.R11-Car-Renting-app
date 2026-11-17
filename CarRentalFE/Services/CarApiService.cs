using System.Net;
using System.Net.Http.Json;
using System.Text;
using CarRentalFE.Models;

namespace CarRentalFE.Services;

// Gọi CarRentalAPI (BE). Địa chỉ lấy từ appsettings: "Api:BaseUrl" (mặc định http://localhost:5110/)
public class CarApiService
{
    private readonly HttpClient _http;
    public CarApiService(HttpClient http) => _http = http;

    // ---------- Xe ----------
    public async Task<PagedResult<CarListItem>> GetCarsAsync(CarQuery q)
    {
        var p = new List<string>();
        void Add(string k, object? v) { if (v != null && v.ToString() != "") p.Add($"{k}={WebUtility.UrlEncode(v.ToString())}"); }
        Add("keyword", q.Keyword); Add("brandId", q.BrandId); Add("categoryId", q.CategoryId);
        Add("minPrice", q.MinPrice); Add("maxPrice", q.MaxPrice); Add("seats", q.Seats);
        Add("status", q.Status); Add("sort", q.Sort);
        Add("pickupDate", q.PickupDate?.ToString("yyyy-MM-dd")); Add("returnDate", q.ReturnDate?.ToString("yyyy-MM-dd")); Add("page", q.Page); Add("pageSize", q.PageSize);
        var url = "cars" + (p.Count > 0 ? "?" + string.Join("&", p) : "");
        return await _http.GetFromJsonAsync<PagedResult<CarListItem>>(url) ?? new PagedResult<CarListItem>();
    }

    public async Task<CarDetailResponse?> GetCarAsync(Guid id)
    {
        var res = await _http.GetAsync($"cars/{id}");
        if (res.StatusCode == HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<CarDetailResponse>();
    }

    // Các hàm ghi trả về (thành công?, thông báo lỗi từ BE)
    // Tạo xe: trả thêm Id của xe mới (để gắn ảnh ngay sau đó)
    public async Task<(bool Ok, string? Error, Guid? Id)> CreateCarAsync(CarSaveRequest r)
    {
        using var res = await _http.PostAsJsonAsync("cars", r);
        if (res.IsSuccessStatusCode)
        {
            var created = await res.Content.ReadFromJsonAsync<CreatedResult>();
            return (true, null, created?.Id);
        }
        return (false, await ReadErrorAsync(res), null);
    }
    public Task<(bool Ok, string? Error)> UpdateCarAsync(Guid id, CarSaveRequest r) => SendAsync(HttpMethod.Put, $"cars/{id}", r);
    public Task<(bool Ok, string? Error)> SetCarStatusAsync(Guid id, string status) => SendAsync(HttpMethod.Patch, $"cars/{id}/status", new { status });
    public Task<(bool Ok, string? Error)> DeleteCarAsync(Guid id) => SendAsync(HttpMethod.Delete, $"cars/{id}", null);
    public Task<(bool Ok, string? Error)> AddCarImageAsync(Guid id, string url, bool isPrimary) =>
        SendAsync(HttpMethod.Post, $"cars/{id}/images", new { url, isPrimary });

    // ---------- Hãng / Loại ----------
    public async Task<List<BrandItem>> GetBrandsAsync() =>
        await _http.GetFromJsonAsync<List<BrandItem>>("car-brands") ?? new();
    public async Task<List<CategoryItem>> GetCategoriesAsync() =>
        await _http.GetFromJsonAsync<List<CategoryItem>>("car-categories") ?? new();

    public Task<(bool Ok, string? Error)> SaveBrandAsync(Guid? id, BrandSaveRequest r) =>
        id == null ? SendAsync(HttpMethod.Post, "car-brands", r) : SendAsync(HttpMethod.Put, $"car-brands/{id}", r);
    public Task<(bool Ok, string? Error)> SaveCategoryAsync(Guid? id, CategorySaveRequest r) =>
        id == null ? SendAsync(HttpMethod.Post, "car-categories", r) : SendAsync(HttpMethod.Put, $"car-categories/{id}", r);

    // ---------- Dashboard ----------
    public async Task<DashboardSummary?> GetDashboardAsync() =>
        await _http.GetFromJsonAsync<DashboardSummary>("admin/dashboard");

    // ---------- Chung ----------
    private async Task<(bool, string?)> SendAsync(HttpMethod method, string url, object? body)
    {
        using var req = new HttpRequestMessage(method, url);
        if (body != null) req.Content = JsonContent.Create(body);
        using var res = await _http.SendAsync(req);
        if (res.IsSuccessStatusCode) return (true, null);
        return (false, await ReadErrorAsync(res));
    }

    // BE trả lỗi dạng { "message": "..." }
    private static async Task<string> ReadErrorAsync(HttpResponseMessage res)
    {
        try
        {
            var err = await res.Content.ReadFromJsonAsync<ApiError>();
            if (!string.IsNullOrWhiteSpace(err?.Message)) return err.Message;
        }
        catch { /* không phải JSON */ }
        return $"Lỗi {(int)res.StatusCode}: {res.ReasonPhrase}";
    }

    private class ApiError { public string? Message { get; set; } }
    private class CreatedResult { public Guid Id { get; set; } }
}
