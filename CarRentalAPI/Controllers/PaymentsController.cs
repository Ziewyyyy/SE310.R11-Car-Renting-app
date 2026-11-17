using System.Net;
using System.Security.Cryptography;
using System.Text;
using CarRentalAPI.Data;
using CarRentalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

public record VnPayCreateDto(Guid ContractId, decimal? Amount);

[ApiController]
[Route("payments")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;
    public PaymentsController(AppDbContext db, IConfiguration cfg) { _db = db; _cfg = cfg; }

    // GET /payments?status=  (Staff, Admin) - kiểm tra giao dịch
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var q = from p in _db.Payments.AsNoTracking()
                join c in _db.Contracts on p.ContractId equals c.Id
                join b in _db.Bookings on c.BookingId equals b.Id
                where status == null || p.Status == status
                orderby p.PaidAt descending
                select new
                {
                    p.Id, p.ContractId, c.BookingId, p.Amount, p.PaidAt, p.Method, p.Status, p.TransactionCode,
                    CustomerName = b.Customer.FullName
                };
        return Ok(await q.ToListAsync());
    }

    // GET /payments/my?customerId=...  (Customer)
    // TODO: khi BE1 xong JWT, bỏ tham số customerId và lấy từ User.FindFirst(ClaimTypes.NameIdentifier)
    [HttpGet("my")]
    public async Task<IActionResult> My([FromQuery] Guid customerId)
    {
        var q = from p in _db.Payments.AsNoTracking()
                join c in _db.Contracts on p.ContractId equals c.Id
                join b in _db.Bookings on c.BookingId equals b.Id
                where b.CustomerId == customerId
                orderby p.PaidAt descending
                select new
                {
                    p.Id, p.ContractId, c.BookingId, p.Amount, p.PaidAt, p.Method, p.Status, p.TransactionCode,
                    CarName = b.Car.Name
                };
        return Ok(await q.ToListAsync());
    }

    // PATCH /payments/{id}/confirm  (Staff, Admin) - xác nhận thủ công (tiền mặt / chuyển khoản)
    [HttpPatch("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var p = await _db.Payments.FindAsync(id);
        if (p == null) return NotFound(new { message = "Không tìm thấy giao dịch." });
        if (p.Status == "Success") return BadRequest(new { message = "Giao dịch đã được xác nhận." });
        p.Status = "Success";
        p.PaidAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { p.Id, p.Status });
    }

    // POST /payments/vnpay/create  (Owner) - tạo URL thanh toán
    [HttpPost("vnpay/create")]
    public async Task<IActionResult> CreateVnPay(VnPayCreateDto dto)
    {
        var tmn = _cfg["VNPay:TmnCode"]; var secret = _cfg["VNPay:HashSecret"];
        var baseUrl = _cfg["VNPay:BaseUrl"]; var returnUrl = _cfg["VNPay:ReturnUrl"];
        if (string.IsNullOrEmpty(tmn) || string.IsNullOrEmpty(secret) ||
            string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(returnUrl))
            return StatusCode(500, new { message = "Chưa cấu hình VNPay (TmnCode, HashSecret, BaseUrl, ReturnUrl)." });

        var contract = await _db.Contracts.FindAsync(dto.ContractId);
        if (contract == null) return NotFound(new { message = "Không tìm thấy hợp đồng." });
        var booking = await _db.Bookings.FindAsync(contract.BookingId);
        if (booking == null) return NotFound(new { message = "Không tìm thấy đơn thuê." });

        // Số tiền: mặc định = cọc nếu chưa trả gì, ngược lại = số còn lại
        var paid = await _db.Payments.Where(x => x.ContractId == contract.Id && x.Status == "Success")
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;
        var remaining = booking.TotalAmount - paid;
        if (remaining <= 0) return BadRequest(new { message = "Hợp đồng đã thanh toán đủ." });
        var amount = dto.Amount ?? (paid == 0 ? contract.DepositAmount : remaining);
        if (amount <= 0 || amount > remaining)
            return BadRequest(new { message = $"Số tiền không hợp lệ (tối đa {remaining:N0})." });

        var payment = new Payment
        {
            ContractId = contract.Id, Amount = amount, Method = "VNPay", Status = "Pending"
        };
        payment.TransactionCode = payment.Id.ToString("N");   // dùng làm vnp_TxnRef
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        var vnNow = DateTime.UtcNow.AddHours(7);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        if (ip.Contains(':')) ip = "127.0.0.1";

        var p = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = tmn,
            ["vnp_Amount"] = ((long)(amount * 100)).ToString(),
            ["vnp_CreateDate"] = vnNow.ToString("yyyyMMddHHmmss"),
            ["vnp_ExpireDate"] = vnNow.AddMinutes(15).ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = ip,
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = $"Thanh toan hop dong {contract.Id:N}",
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_TxnRef"] = payment.TransactionCode
        };
        var query = BuildQuery(p);
        var url = $"{baseUrl}?{query}&vnp_SecureHash={HmacSha512(secret, query)}";
        return Ok(new { paymentId = payment.Id, paymentUrl = url });
    }

    // GET /payments/vnpay/return  (Public, verify signature) - trình duyệt quay lại
    [HttpGet("vnpay/return")]
    public async Task<IActionResult> VnPayReturn()
    {
        var r = await Process();
        return Ok(new { success = r.Code == "00", message = r.Message, code = r.Code });
    }

    // GET /payments/vnpay/ipn  (VNPay gọi server-to-server)
    [HttpGet("vnpay/ipn")]
    public async Task<IActionResult> VnPayIpn()
    {
        var r = await Process();
        return Ok(new { RspCode = r.Code, Message = r.Message });
    }

    private async Task<(string Code, string Message)> Process()
    {
        var secret = _cfg["VNPay:HashSecret"];
        if (string.IsNullOrEmpty(secret)) return ("99", "Chưa cấu hình VNPay");

        var p = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var kv in Request.Query)
            if (kv.Key.StartsWith("vnp_") && kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                p[kv.Key] = kv.Value.ToString();

        var receivedHash = Request.Query["vnp_SecureHash"].ToString();
        if (!string.Equals(HmacSha512(secret, BuildQuery(p)), receivedHash, StringComparison.OrdinalIgnoreCase))
            return ("97", "Invalid signature");

        var txnRef = p.GetValueOrDefault("vnp_TxnRef", "");
        var payment = await _db.Payments.FirstOrDefaultAsync(x => x.TransactionCode == txnRef);
        if (payment == null) return ("01", "Order not found");

        long.TryParse(p.GetValueOrDefault("vnp_Amount", "0"), out var vnpAmount);
        if (vnpAmount != (long)(payment.Amount * 100)) return ("04", "Invalid amount");
        if (payment.Status != "Pending") return ("02", "Order already confirmed");

        var ok = p.GetValueOrDefault("vnp_ResponseCode") == "00" &&
                 p.GetValueOrDefault("vnp_TransactionStatus") == "00";
        payment.Status = ok ? "Success" : "Failed";
        if (ok) payment.PaidAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ("00", ok ? "Confirm Success" : "Payment failed");
    }

    private static string BuildQuery(SortedDictionary<string, string> p) =>
        string.Join("&", p.Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

    private static string HmacSha512(string key, string data)
    {
        using var h = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLower();
    }
}
