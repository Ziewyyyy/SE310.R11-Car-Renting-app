using CarRentalAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    // GET /admin/dashboard  (Staff, Admin)
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var pending = await _db.Bookings.CountAsync(b => b.Status == "Pending");
        var renting = await _db.Bookings.CountAsync(b => b.Status == "Confirmed" && b.ActualReturnDate == null
                                                          && b.PickupDate <= now);
        var availableCars = await _db.Cars.CountAsync(c => c.Status == "Available");
        var totalCars = await _db.Cars.CountAsync(c => c.Status != "Inactive");
        var pendingExt = await _db.RentalExtensions.CountAsync(e => e.Status == "Pending");
        var openSupport = await _db.SupportRequests.CountAsync(s => s.Status != "Closed" && s.Status != "Resolved");
        var revenueMonth = await _db.Payments
            .Where(p => p.Status == "Success" && p.PaidAt >= monthStart)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var latestPending = await _db.Bookings.AsNoTracking().Where(b => b.Status == "Pending")
            .OrderBy(b => b.PickupDate).Take(5)
            .Select(b => new
            {
                b.Id, CustomerName = b.Customer.FullName, CarName = b.Car.Name,
                b.PickupDate, b.ReturnDate, b.TotalAmount
            }).ToListAsync();

        return Ok(new
        {
            pendingBookings = pending,
            rentingCars = renting,
            availableCars,
            totalCars,
            pendingExtensions = pendingExt,
            openSupportRequests = openSupport,
            revenueThisMonth = revenueMonth,
            latestPendingBookings = latestPending
        });
    }

    // GET /admin/reports?from=2026-10-01&to=2026-11-30  (Admin)
    [HttpGet("reports")]
    public async Task<IActionResult> Reports([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var f = Common.Utc(from ?? DateTime.UtcNow.AddMonths(-6));
        var t = Common.Utc(to ?? DateTime.UtcNow);

        var payments = await _db.Payments.AsNoTracking()
            .Where(p => p.Status == "Success" && p.PaidAt >= f && p.PaidAt <= t)
            .Select(p => new { p.PaidAt, p.Amount }).ToListAsync();

        var revenueByMonth = payments
            .GroupBy(p => new { p.PaidAt!.Value.Year, p.PaidAt.Value.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new { month = $"{g.Key.Year}-{g.Key.Month:00}", revenue = g.Sum(x => x.Amount) })
            .ToList();

        var bookingsByStatus = await _db.Bookings.AsNoTracking()
            .Where(b => b.CreatedAt >= f && b.CreatedAt <= t)
            .GroupBy(b => b.Status)
            .Select(g => new { status = g.Key, count = g.Count() }).ToListAsync();

        var topCars = await _db.Bookings.AsNoTracking()
            .Where(b => b.Status != "Cancelled" && b.Status != "Rejected" && b.CreatedAt >= f && b.CreatedAt <= t)
            .GroupBy(b => new { b.CarId, b.Car.Name })
            .Select(g => new { carId = g.Key.CarId, name = g.Key.Name, bookings = g.Count(), amount = g.Sum(b => b.TotalAmount) })
            .OrderByDescending(x => x.bookings).Take(5).ToListAsync();

        return Ok(new
        {
            from = f, to = t,
            totalRevenue = payments.Sum(p => p.Amount),
            revenueByMonth, bookingsByStatus, topCars
        });
    }
}
