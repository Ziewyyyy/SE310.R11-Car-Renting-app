using CarRentalAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS: cho phép FE (Blazor) gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFE", policy =>
        policy.WithOrigins(
                "http://localhost:5088",
                "https://localhost:7283")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFE");

app.MapControllers();

app.MapGet("/", () => "Backend is running!");

app.Run();