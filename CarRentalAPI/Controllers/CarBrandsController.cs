using CarRentalAPI.Data;
using CarRentalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

public record BrandDto(string Name, string? Country);
public record CategoryDto(string Name, string? Description);

[ApiController]
[Route("car-brands")]
public class CarBrandsController : ControllerBase
{
    private readonly AppDbContext _db;
    public CarBrandsController(AppDbContext db) => _db = db;

    // Trả thêm CarCount (số xe chưa ngừng hoạt động của hãng)
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.CarBrands.AsNoTracking().OrderBy(b => b.Name)
            .Select(b => new
            {
                b.Id, b.Name, b.Country,
                CarCount = b.Cars.Count(c => c.Status != "Inactive")
            }).ToListAsync());

    // TODO: [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(BrandDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new { message = "Tên hãng không được trống." });
        var name = dto.Name.Trim();
        if (await _db.CarBrands.AnyAsync(b => b.Name.ToLower() == name.ToLower()))
            return Conflict(new { message = "Hãng xe đã tồn tại." });
        var b = new CarBrand { Name = name, Country = dto.Country?.Trim() ?? "" };
        _db.CarBrands.Add(b);
        await _db.SaveChangesAsync();
        return Created($"/car-brands/{b.Id}", new { b.Id, b.Name, b.Country });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, BrandDto dto)
    {
        var b = await _db.CarBrands.FindAsync(id);
        if (b == null) return NotFound(new { message = "Không tìm thấy hãng xe." });
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new { message = "Tên hãng không được trống." });
        var name = dto.Name.Trim();
        if (await _db.CarBrands.AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower()))
            return Conflict(new { message = "Tên hãng đã tồn tại." });
        b.Name = name; b.Country = dto.Country?.Trim() ?? "";
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // MỚI: xóa hãng (chỉ khi chưa có xe nào thuộc hãng, kể cả xe đã ngừng hoạt động)
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var b = await _db.CarBrands.FindAsync(id);
        if (b == null) return NotFound(new { message = "Không tìm thấy hãng xe." });
        if (await _db.Cars.AnyAsync(c => c.BrandId == id))
            return Conflict(new { message = "Hãng xe đang có xe, không thể xóa." });
        _db.CarBrands.Remove(b);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

[ApiController]
[Route("car-categories")]
public class CarCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CarCategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.CarCategories.AsNoTracking().OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id, c.Name, c.Description,
                CarCount = c.Cars.Count(x => x.Status != "Inactive")
            }).ToListAsync());

    // TODO: [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new { message = "Tên loại không được trống." });
        var name = dto.Name.Trim();
        if (await _db.CarCategories.AnyAsync(c => c.Name.ToLower() == name.ToLower()))
            return Conflict(new { message = "Loại xe đã tồn tại." });
        var c = new CarCategory { Name = name, Description = dto.Description?.Trim() ?? "" };
        _db.CarCategories.Add(c);
        await _db.SaveChangesAsync();
        return Created($"/car-categories/{c.Id}", new { c.Id, c.Name, c.Description });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CategoryDto dto)
    {
        var c = await _db.CarCategories.FindAsync(id);
        if (c == null) return NotFound(new { message = "Không tìm thấy loại xe." });
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new { message = "Tên loại không được trống." });
        var name = dto.Name.Trim();
        if (await _db.CarCategories.AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower()))
            return Conflict(new { message = "Tên loại đã tồn tại." });
        c.Name = name; c.Description = dto.Description?.Trim() ?? "";
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // MỚI: xóa loại xe (chỉ khi chưa có xe nào thuộc loại)
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var c = await _db.CarCategories.FindAsync(id);
        if (c == null) return NotFound(new { message = "Không tìm thấy loại xe." });
        if (await _db.Cars.AnyAsync(x => x.CategoryId == id))
            return Conflict(new { message = "Loại xe đang có xe, không thể xóa." });
        _db.CarCategories.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
