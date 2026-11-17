using CarRentalAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

public record SupportStatusDto(string Status, Guid? StaffId);

[ApiController]
[Route("support-requests")]
public class SupportRequestsController : ControllerBase
{
    private static readonly string[] AllowedStatuses = { "Open", "InProgress", "Resolved", "Closed" };
    private readonly AppDbContext _db;
    public SupportRequestsController(AppDbContext db) => _db = db;

    // GET /support-requests?status=  (Staff, Admin)
    // BE1 làm POST /support-requests trong controller của họ: nhớ trùng route "support-requests"
    // thì gộp vào một file để tránh trùng tên class.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var q = from s in _db.SupportRequests.AsNoTracking()
                join u in _db.Users on s.CustomerId equals u.Id
                where status == null || s.Status == status
                orderby s.CreatedAt descending
                select new
                {
                    s.Id, s.Title, s.Content, s.Status, s.CreatedAt, s.StaffId,
                    CustomerId = u.Id, CustomerName = u.FullName, CustomerEmail = u.Email
                };
        return Ok(await q.ToListAsync());
    }

    // PATCH /support-requests/{id}/status  (Staff, Admin) - tiếp nhận / cập nhật xử lý
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, SupportStatusDto dto)
    {
        if (!AllowedStatuses.Contains(dto.Status))
            return BadRequest(new { message = $"Trạng thái hợp lệ: {string.Join(", ", AllowedStatuses)}" });
        var s = await _db.SupportRequests.FindAsync(id);
        if (s == null) return NotFound(new { message = "Không tìm thấy yêu cầu hỗ trợ." });
        s.Status = dto.Status;
        if (dto.StaffId.HasValue) s.StaffId = dto.StaffId;   // TODO: lấy từ JWT khi có
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
