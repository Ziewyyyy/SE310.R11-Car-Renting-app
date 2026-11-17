using CarRentalAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    // GET /users?role=&keyword=  (Admin)
    // Lưu ý: BE1 sẽ làm /users/me trong controller riêng (route "users/me" không trùng với route ở đây)
    // TODO: [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? role, [FromQuery] string? keyword)
    {
        var q = _db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(role)) q = q.Where(u => u.Role == role);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim().ToLower();
            q = q.Where(u => u.FullName.ToLower().Contains(k) || u.Email.ToLower().Contains(k));
        }
        return Ok(await q.OrderBy(u => u.FullName)
            .Select(u => new { u.Id, u.FullName, u.Email, u.Phone, u.Role }).ToListAsync());
    }
}
