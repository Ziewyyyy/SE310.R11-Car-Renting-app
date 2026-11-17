
using CarRentalFE.Components;
using CarRentalFE.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? "http://localhost:5110/";

// AuthService
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Protected storage để lưu token
builder.Services.AddScoped<ProtectedLocalStorage>();

// Các service API khác
builder.Services.AddHttpClient<CarApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
