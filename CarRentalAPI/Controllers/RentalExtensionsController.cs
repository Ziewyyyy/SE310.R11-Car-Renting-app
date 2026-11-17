using CarRentalAPI.Data;
using CarRentalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

public record ExtensionCreateDto(Guid BookingId, DateTime NewReturnDate);
public record ExtensionReviewDto(bool Approve);

[ApiController]
[Route("rental-extensions")]
public class RentalExtensionsController : ControllerBase
{
    private readonly AppDbContext _db;
    public RentalExtensionsController(AppDbContext db) => _db = db;

    // GET /rental-extensions?status=Pending  (Staff, Admin)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var q = from e in _db.RentalExtensions.AsNoTracking()
                join b in _db.Bookings on e.BookingId equals b.Id
                where status == null || e.Status == status
                orderby e.RequestedAt descending
                select new
                {
                    e.Id, e.BookingId, e.NewReturnDate, e.ExtraAmount, e.Status, e.RequestedAt,
                    CurrentReturnDate = b.ReturnDate,
                    CustomerName = b.Customer.FullName, CarName = b.Car.Name
                };
        return Ok(await q.ToListAsync());
    }

    // GET /rental-extensions/booking/{bookingId}  - khách xem các yêu cầu của đơn mình
    [HttpGet("booking/{bookingId:guid}")]
    public async Task<IActionResult> GetByBooking(Guid bookingId) =>
        Ok(await _db.RentalExtensions.AsNoTracking().Where(e => e.BookingId == bookingId)
            .OrderByDescending(e => e.RequestedAt).ToListAsync());

    // POST /rental-extensions  (Owner) TODO: kiểm tra Owner khi có JWT
    [HttpPost]
    public async Task<IActionResult> Create(ExtensionCreateDto dto)
    {
        var booking = await _db.Bookings.Include(b => b.Car).FirstOrDefaultAsync(b => b.Id == dto.BookingId);
        if (booking == null) return NotFound(new { message = "Không tìm thấy đơn thuê." });
        if (booking.Status != "Confirmed")
            return BadRequest(new { message = "Chỉ gia hạn được đơn đang thuê (Confirmed)." });

        var newDate = Common.Utc(dto.NewReturnDate);
        if (newDate <= booking.ReturnDate)
            return BadRequest(new { message = "Ngày trả mới phải sau ngày trả hiện tại." });
        if (await _db.RentalExtensions.AnyAsync(e => e.BookingId == booking.Id && e.Status == "Pending"))
            return Conflict(new { message = "Đơn này đang có yêu cầu gia hạn chờ duyệt." });

        // Xe phải chưa có đơn khác trong khoảng gia hạn
        var conflict = await _db.Bookings.AnyAsync(b => b.CarId == booking.CarId && b.Id != booking.Id &&
            (b.Status == "Pending" || b.Status == "Confirmed") &&
            b.PickupDate < newDate && b.ReturnDate > booking.ReturnDate);
        if (conflict) return Conflict(new { message = "Xe đã có người đặt trong khoảng thời gian gia hạn." });

        var extraDays = (int)Math.Ceiling((newDate - booking.ReturnDate).TotalDays);
        var ext = new RentalExtension
        {
            BookingId = booking.Id,
            NewReturnDate = newDate,
            ExtraAmount = extraDays * booking.Car.PricePerDay,
            Status = "Pending"
        };
        _db.RentalExtensions.Add(ext);
        await _db.SaveChangesAsync();
        return Created($"/rental-extensions/{ext.Id}", new { ext.Id, ext.ExtraAmount, ext.Status });
    }

    // PATCH /rental-extensions/{id}/review  (Staff, Admin)
    [HttpPatch("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, ExtensionReviewDto dto)
    {
        var ext = await _db.RentalExtensions.FindAsync(id);
        if (ext == null) return NotFound(new { message = "Không tìm thấy yêu cầu gia hạn." });
        if (ext.Status != "Pending") return BadRequest(new { message = "Yêu cầu này đã được xử lý." });

        if (!dto.Approve)
        {
            ext.Status = "Rejected";
        }
        else
        {
            var booking = await _db.Bookings.FindAsync(ext.BookingId);
            if (booking == null) return NotFound(new { message = "Không tìm thấy đơn thuê." });
            booking.ReturnDate = ext.NewReturnDate;
            booking.TotalAmount += ext.ExtraAmount;
            ext.Status = "Approved";
        }
        await _db.SaveChangesAsync();
        return Ok(new { ext.Id, ext.Status });
    }
}
