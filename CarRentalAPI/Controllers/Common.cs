namespace CarRentalAPI.Controllers;

public static class Common
{
    // Npgsql (timestamptz) chỉ nhận DateTime kiểu UTC, hàm này đổi mọi giá trị về UTC
    public static DateTime Utc(DateTime d) =>
        d.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(d, DateTimeKind.Utc)
            : d.ToUniversalTime();

    // Trạng thái đơn còn "giữ xe"
    public static readonly string[] ActiveBookingStatuses = { "Pending", "Confirmed" };
}
