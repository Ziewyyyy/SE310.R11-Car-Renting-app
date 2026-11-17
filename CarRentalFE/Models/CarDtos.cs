namespace CarRentalFE.Models;

// Khớp với JSON mà CarRentalAPI trả về (camelCase, đọc không phân biệt hoa thường)

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class CarListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string LicensePlate { get; set; } = "";
    public int ManufactureYear { get; set; }
    public decimal PricePerDay { get; set; }
    public string Color { get; set; } = "";
    public int Seats { get; set; }
    public string Status { get; set; } = "";
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = "";
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string? PrimaryImage { get; set; }
    public double? AverageRating { get; set; }
}

public class CarDetailResponse
{
    public CarDetail Car { get; set; } = new();
    public List<CarImageItem> Images { get; set; } = new();
    public List<CarFeatureItem> Features { get; set; } = new();
}

public class CarDetail
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string LicensePlate { get; set; } = "";
    public int ManufactureYear { get; set; }
    public decimal PricePerDay { get; set; }
    public string Color { get; set; } = "";
    public int Seats { get; set; }
    public int Mileage { get; set; }
    public string Status { get; set; } = "";
    public BrandItem Brand { get; set; } = new();
    public CategoryItem Category { get; set; } = new();
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

public class BrandItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";
}

public class CategoryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

public class CarImageItem
{
    public Guid Id { get; set; }
    public string Url { get; set; } = "";
    public bool IsPrimary { get; set; }
}

public class CarFeatureItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

// Bộ lọc gửi lên GET /cars
public class CarQuery
{
    public string? Keyword { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? Seats { get; set; }
    public string? Status { get; set; }
    public string? Sort { get; set; }
    public DateTime? PickupDate { get; set; }   // lọc xe còn trống trong khoảng ngày
    public DateTime? ReturnDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;
}

// Body gửi POST/PUT /cars
public class CarSaveRequest
{
    public string Name { get; set; } = "";
    public string LicensePlate { get; set; } = "";
    public int ManufactureYear { get; set; } = DateTime.Now.Year;
    public decimal PricePerDay { get; set; }
    public string Color { get; set; } = "";
    public int Seats { get; set; } = 5;
    public int Mileage { get; set; }
    public string? Status { get; set; } = "Available";
    public Guid BrandId { get; set; }
    public Guid CategoryId { get; set; }
}

// Body gửi POST/PUT /car-brands và /car-categories
public class BrandSaveRequest
{
    public string Name { get; set; } = "";
    public string? Country { get; set; }
}

public class CategorySaveRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public class DashboardSummary
{
    public int PendingBookings { get; set; }
    public int RentingCars { get; set; }
    public int AvailableCars { get; set; }
    public int TotalCars { get; set; }
}
