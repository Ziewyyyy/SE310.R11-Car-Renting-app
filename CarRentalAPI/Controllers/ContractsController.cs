using CarRentalAPI.Data;
using CarRentalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

public record ContractCreateDto(Guid BookingId, decimal? DepositAmount);
public record ContractStatusDto(string Status);

[ApiController]
[Route("contracts")]
public class ContractsController : ControllerBase
{
    private static readonly string[] AllowedStatuses = { "Active", "Completed", "Cancelled" };
    private readonly AppDbContext _db;
    public ContractsController(AppDbContext db) => _db = db;

    // GET /contracts?status=  (Staff, Admin) - danh sách hợp đồng
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var q = from c in _db.Contracts.AsNoTracking()
                join b in _db.Bookings on c.BookingId equals b.Id
                where status == null || c.Status == status
                orderby c.SignedAt descending
                select new
                {
                    c.Id, c.BookingId, c.SignedAt, c.StartDate, c.EndDate, c.DepositAmount, c.Status,
                    CustomerName = b.Customer.FullName, CarName = b.Car.Name, b.TotalAmount
                };
        return Ok(await q.ToListAsync());
    }

    // GET /contracts/{id}  (Owner, Staff, Admin)  TODO: kiểm tra Owner khi có JWT
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var q = from c in _db.Contracts.AsNoTracking()
                join b in _db.Bookings on c.BookingId equals b.Id
                where c.Id == id
                select new
                {
                    c.Id, c.BookingId, c.SignedAt, c.StartDate, c.EndDate, c.DepositAmount, c.Status,
                    Booking = new { b.Id, b.PickupDate, b.ReturnDate, b.ActualReturnDate, b.TotalAmount, b.Status },
                    Customer = new { b.Customer.Id, b.Customer.FullName, b.Customer.Email, b.Customer.Phone },
                    Car = new { b.Car.Id, b.Car.Name, b.Car.LicensePlate, b.Car.PricePerDay }
                };
        var result = await q.FirstOrDefaultAsync();
        return result == null ? NotFound(new { message = "Không tìm thấy hợp đồng." }) : Ok(result);
    }

    // GET /contracts/by-booking/{bookingId}  - khách xem hợp đồng của đơn
    [HttpGet("by-booking/{bookingId:guid}")]
    public async Task<IActionResult> GetByBooking(Guid bookingId)
    {
        var c = await _db.Contracts.AsNoTracking().FirstOrDefaultAsync(x => x.BookingId == bookingId);
        return c == null ? NotFound(new { message = "Đơn này chưa có hợp đồng." }) : Ok(c);
    }

    // POST /contracts  (Staff, Admin) - chỉ tạo cho đơn đã Confirmed
    [HttpPost]
    public async Task<IActionResult> Create(ContractCreateDto dto)
    {
        var booking = await _db.Bookings.FindAsync(dto.BookingId);
        if (booking == null) return NotFound(new { message = "Không tìm thấy đơn thuê." });
        if (booking.Status != "Confirmed")
            return BadRequest(new { message = "Chỉ tạo hợp đồng cho đơn đã được xác nhận (Confirmed)." });
        if (await _db.Contracts.AnyAsync(c => c.BookingId == dto.BookingId))
            return Conflict(new { message = "Đơn này đã có hợp đồng." });

        // Mặc định cọc 20% tổng tiền, làm tròn đến nghìn đồng
        var deposit = dto.DepositAmount ?? Math.Round(booking.TotalAmount * 0.2m / 1000m) * 1000m;
        if (deposit < 0 || deposit > booking.TotalAmount)
            return BadRequest(new { message = "Tiền cọc không hợp lệ." });

        var contract = new Contract
        {
            BookingId = booking.Id,
            SignedAt = DateTime.UtcNow,
            StartDate = booking.PickupDate,
            EndDate = booking.ReturnDate,
            DepositAmount = deposit,
            Status = "Active"
        };
        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync();
        return Created($"/contracts/{contract.Id}", new { contract.Id, contract.DepositAmount, contract.Status });
    }

    // PATCH /contracts/{id}/status  (Staff, Admin) - cập nhật hợp đồng
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, ContractStatusDto dto)
    {
        if (!AllowedStatuses.Contains(dto.Status))
            return BadRequest(new { message = $"Trạng thái hợp lệ: {string.Join(", ", AllowedStatuses)}" });
        var c = await _db.Contracts.FindAsync(id);
        if (c == null) return NotFound(new { message = "Không tìm thấy hợp đồng." });
        c.Status = dto.Status;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
