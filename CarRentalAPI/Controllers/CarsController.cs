using CarRentalAPI.Data;
using CarRentalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

public class CarFilter
{
    public string? Keyword { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? Seats { get; set; }
    public string? Status { get; set; }          // bỏ trống = mọi xe chưa ngừng hoạt động
    public DateTime? PickupDate { get; set; }    // lọc xe còn trống trong khoảng ngày
    public DateTime? ReturnDate { get; set; }
    public string? Sort { get; set; }            // price_asc | price_desc | newest
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public record CarListItemDto(
    Guid Id, string Name, string LicensePlate, int ManufactureYear, decimal PricePerDay,
    string Color, int Seats, string Status,
    Guid BrandId, string BrandName, Guid CategoryId, string CategoryName,
    string? PrimaryImage, double? AverageRating);

public record CarUpsertDto(
    string Name, string LicensePlate, int ManufactureYear, decimal PricePerDay,
    string Color, int Seats, int Mileage, string? Status, Guid BrandId, Guid CategoryId);

public record CarStatusDto(string Status);
public record CarImageDto(string Url, bool IsPrimary);

[ApiController]
[Route("cars")]
public class CarsController : ControllerBase
{
    private static readonly string[] AllowedStatuses = { "Available", "Rented", "Maintenance", "Inactive" };
    private readonly AppDbContext _db;
    public CarsController(AppDbContext db) => _db = db;

    // GET /cars  (Public) - tìm / lọc xe
    [HttpGet]
    public async Task<IActionResult> GetCars([FromQuery] CarFilter f)
    {
        var q = _db.Cars.AsNoTracking().AsQueryable();

        q = string.IsNullOrWhiteSpace(f.Status)
            ? q.Where(c => c.Status != "Inactive")
            : q.Where(c => c.Status == f.Status);

        if (!string.IsNullOrWhiteSpace(f.Keyword))
        {
            var k = f.Keyword.Trim().ToLower();
            q = q.Where(c => c.Name.ToLower().Contains(k) || c.LicensePlate.ToLower().Contains(k));
        }
        if (f.BrandId.HasValue) q = q.Where(c => c.BrandId == f.BrandId);
        if (f.CategoryId.HasValue) q = q.Where(c => c.CategoryId == f.CategoryId);
        if (f.MinPrice.HasValue) q = q.Where(c => c.PricePerDay >= f.MinPrice);
        if (f.MaxPrice.HasValue) q = q.Where(c => c.PricePerDay <= f.MaxPrice);
        if (f.Seats.HasValue) q = q.Where(c => c.Seats == f.Seats);

        if (f.PickupDate.HasValue && f.ReturnDate.HasValue)
        {
            var from = Common.Utc(f.PickupDate.Value);
            var to = Common.Utc(f.ReturnDate.Value);
            q = q.Where(c => c.Status == "Available" &&
                !c.Bookings.Any(b => (b.Status == "Pending" || b.Status == "Confirmed")
                                     && b.PickupDate < to && b.ReturnDate > from));
        }

        q = f.Sort switch
        {
            "price_asc" => q.OrderBy(c => c.PricePerDay),
            "price_desc" => q.OrderByDescending(c => c.PricePerDay),
            "newest" => q.OrderByDescending(c => c.ManufactureYear),
            _ => q.OrderBy(c => c.Name)
        };

        var page = Math.Max(1, f.Page);
        var size = Math.Clamp(f.PageSize, 1, 100);
        var total = await q.CountAsync();

        var items = await q.Skip((page - 1) * size).Take(size)
            .Select(c => new CarListItemDto(
                c.Id, c.Name, c.LicensePlate, c.ManufactureYear, c.PricePerDay,
                c.Color, c.Seats, c.Status,
                c.BrandId, c.Brand.Name, c.CategoryId, c.Category.Name,
                _db.CarImages.Where(i => i.CarId == c.Id)
                    .OrderByDescending(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault(),
                _db.Reviews.Where(r => r.CarId == c.Id).Average(r => (double?)r.Rating)))
            .ToListAsync();

        return Ok(new { items, total, page, pageSize = size });
    }

    // GET /cars/{id}  (Public)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCar(Guid id)
    {
        var car = await _db.Cars.AsNoTracking().Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id, c.Name, c.LicensePlate, c.ManufactureYear, c.PricePerDay,
                c.Color, c.Seats, c.Mileage, c.Status,
                Brand = new { c.Brand.Id, c.Brand.Name, c.Brand.Country },
                Category = new { c.Category.Id, c.Category.Name, c.Category.Description },
                AverageRating = _db.Reviews.Where(r => r.CarId == c.Id).Average(r => (double?)r.Rating),
                ReviewCount = _db.Reviews.Count(r => r.CarId == c.Id)
            }).FirstOrDefaultAsync();
        if (car == null) return NotFound(new { message = "Không tìm thấy xe." });

        var images = await _db.CarImages.AsNoTracking().Where(i => i.CarId == id)
            .OrderByDescending(i => i.IsPrimary).Select(i => new { i.Id, i.Url, i.IsPrimary }).ToListAsync();

        var features = await (from m in _db.CarFeatureMappings
                              join ft in _db.CarFeatures on m.CarFeatureId equals ft.Id
                              where m.CarId == id
                              select new { ft.Id, ft.Name, ft.Description }).AsNoTracking().ToListAsync();

        return Ok(new { car, images, features });
    }

    // POST /cars  (Staff, Admin)  TODO: [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CarUpsertDto dto)
    {
        var err = await Validate(dto, null);
        if (err != null) return BadRequest(new { message = err });

        var car = new Car
        {
            Name = dto.Name.Trim(), LicensePlate = dto.LicensePlate.Trim(),
            ManufactureYear = dto.ManufactureYear, PricePerDay = dto.PricePerDay,
            Color = dto.Color, Seats = dto.Seats, Mileage = dto.Mileage,
            Status = dto.Status ?? "Available", BrandId = dto.BrandId, CategoryId = dto.CategoryId
        };
        _db.Cars.Add(car);
        await _db.SaveChangesAsync();
        return Created($"/cars/{car.Id}", new { car.Id });
    }

    // PUT /cars/{id}  (Staff, Admin)
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CarUpsertDto dto)
    {
        var car = await _db.Cars.FindAsync(id);
        if (car == null) return NotFound(new { message = "Không tìm thấy xe." });

        var err = await Validate(dto, id);
        if (err != null) return BadRequest(new { message = err });

        car.Name = dto.Name.Trim(); car.LicensePlate = dto.LicensePlate.Trim();
        car.ManufactureYear = dto.ManufactureYear; car.PricePerDay = dto.PricePerDay;
        car.Color = dto.Color; car.Seats = dto.Seats; car.Mileage = dto.Mileage;
        car.BrandId = dto.BrandId; car.CategoryId = dto.CategoryId;
        if (!string.IsNullOrWhiteSpace(dto.Status)) car.Status = dto.Status;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PATCH /cars/{id}/status  (Staff, Admin) - cập nhật trạng thái nhanh
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, CarStatusDto dto)
    {
        if (!AllowedStatuses.Contains(dto.Status))
            return BadRequest(new { message = $"Trạng thái hợp lệ: {string.Join(", ", AllowedStatuses)}" });
        var car = await _db.Cars.FindAsync(id);
        if (car == null) return NotFound(new { message = "Không tìm thấy xe." });
        car.Status = dto.Status;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /cars/{id}  (Admin) - xóa mềm: chuyển sang Inactive
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var car = await _db.Cars.FindAsync(id);
        if (car == null) return NotFound(new { message = "Không tìm thấy xe." });

        var hasActive = await _db.Bookings.AnyAsync(b => b.CarId == id &&
            (b.Status == "Pending" || b.Status == "Confirmed"));
        if (hasActive)
            return Conflict(new { message = "Xe đang có đơn thuê chưa hoàn tất, không thể ngừng hoạt động." });

        car.Status = "Inactive";
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET /cars/{id}/images  (Public)
    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages(Guid id)
    {
        if (!await _db.Cars.AnyAsync(c => c.Id == id)) return NotFound(new { message = "Không tìm thấy xe." });
        var images = await _db.CarImages.AsNoTracking().Where(i => i.CarId == id)
            .OrderByDescending(i => i.IsPrimary).Select(i => new { i.Id, i.Url, i.IsPrimary }).ToListAsync();
        return Ok(images);
    }

    // POST /cars/{id}/images  (Staff, Admin)
    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> AddImage(Guid id, CarImageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Url)) return BadRequest(new { message = "Url ảnh không được trống." });
        if (!await _db.Cars.AnyAsync(c => c.Id == id)) return NotFound(new { message = "Không tìm thấy xe." });

        if (dto.IsPrimary)
        {
            var old = await _db.CarImages.Where(i => i.CarId == id && i.IsPrimary).ToListAsync();
            old.ForEach(i => i.IsPrimary = false);
        }
        var img = new CarImage { CarId = id, Url = dto.Url.Trim(), IsPrimary = dto.IsPrimary };
        _db.CarImages.Add(img);
        await _db.SaveChangesAsync();
        return Created($"/cars/{id}/images", new { img.Id, img.Url, img.IsPrimary });
    }

    // DELETE /cars/{id}/images/{imageId}  (Staff, Admin)
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId)
    {
        var img = await _db.CarImages.FirstOrDefaultAsync(i => i.Id == imageId && i.CarId == id);
        if (img == null) return NotFound(new { message = "Không tìm thấy ảnh." });
        _db.CarImages.Remove(img);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> Validate(CarUpsertDto d, Guid? selfId)
    {
        if (string.IsNullOrWhiteSpace(d.Name)) return "Tên xe không được trống.";
        if (string.IsNullOrWhiteSpace(d.LicensePlate)) return "Biển số không được trống.";
        if (d.PricePerDay <= 0) return "Giá thuê/ngày phải lớn hơn 0.";
        if (d.Seats <= 0) return "Số chỗ phải lớn hơn 0.";
        if (d.ManufactureYear < 1990 || d.ManufactureYear > DateTime.UtcNow.Year + 1) return "Năm sản xuất không hợp lệ.";
        if (d.Mileage < 0) return "Số km không hợp lệ.";
        if (!string.IsNullOrWhiteSpace(d.Status) && !AllowedStatuses.Contains(d.Status))
            return $"Trạng thái hợp lệ: {string.Join(", ", AllowedStatuses)}";
        if (!await _db.CarBrands.AnyAsync(b => b.Id == d.BrandId)) return "Hãng xe không tồn tại.";
        if (!await _db.CarCategories.AnyAsync(c => c.Id == d.CategoryId)) return "Loại xe không tồn tại.";
        var plate = d.LicensePlate.Trim();
        if (await _db.Cars.AnyAsync(c => c.LicensePlate == plate && c.Id != selfId)) return "Biển số đã tồn tại.";
        return null;
    }
}
