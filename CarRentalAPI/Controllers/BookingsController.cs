using CarRentalAPI.Data;
using CarRentalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

public record BookingCreateDto(Guid CustomerId, Guid CarId, DateTime PickupDate, DateTime ReturnDate);

[ApiController]
[Route("bookings")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    public BookingsController(AppDbContext db) => _db = db;

    // POST /bookings  TODO: lấy CustomerId từ JWT khi bật xác thực
    [HttpPost]
    public async Task<IActionResult> Create(BookingCreateDto dto)
    {
        var from = Common.Utc(dto.PickupDate);
        var to = Common.Utc(dto.ReturnDate);

        if (to <= from)
            return BadRequest(new { message = "Ngày trả xe phải sau ngày nhận xe." });
        if (from.Date < DateTime.UtcNow.Date)
            return BadRequest(new { message = "Ngày nhận xe không thể ở quá khứ." });

        if (!await _db.Users.AnyAsync(u => u.Id == dto.CustomerId && u.Role == "Customer"))
            return NotFound(new { message = "Không tìm thấy khách hàng." });

        var car = await _db.Cars.FindAsync(dto.CarId);
        if (car == null) return NotFound(new { message = "Không tìm thấy xe." });
        if (car.Status != "Available")
            return Conflict(new { message = "Xe hiện không sẵn sàng cho thuê." });

        var overlap = await _db.Bookings.AnyAsync(b => b.CarId == dto.CarId &&
            (b.Status == "Pending" || b.Status == "Confirmed") &&
            b.PickupDate < to && b.ReturnDate > from);
        if (overlap)
            return Conflict(new { message = "Xe đã có người đặt trong khoảng thời gian này." });

        var days = Math.Max(1, (int)Math.Ceiling((to - from).TotalDays));

        var booking = new Booking
        {
            CustomerId = dto.CustomerId,
            CarId = dto.CarId,
            PickupDate = from,
            ReturnDate = to,
            TotalAmount = car.PricePerDay * days,
            Status = "Pending"
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        return Created($"/bookings/{booking.Id}", new { booking.Id, booking.TotalAmount, booking.Status });
    }

    // GET /bookings/my?customerId=...  TODO: bỏ tham số khi có JWT
    [HttpGet("my")]
    public async Task<IActionResult> My([FromQuery] Guid customerId)
    {
        var list = await _db.Bookings.AsNoTracking()
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new
            {
                b.Id,
                b.PickupDate,
                b.ReturnDate,
                b.TotalAmount,
                b.Status,
                b.CreatedAt,
                CarId = b.CarId,
                CarName = b.Car.Name,
                LicensePlate = b.Car.LicensePlate,
                PrimaryImage = _db.CarImages.Where(i => i.CarId == b.CarId)
        .OrderByDescending(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault()
            }).ToListAsync();
        return Ok(list);
    }
}