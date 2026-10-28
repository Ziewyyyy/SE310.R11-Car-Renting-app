
using System.ComponentModel.DataAnnotations;

namespace CarRentalFE.Models;

public class RegisterRequest
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^0[0-9]{9}$",
        ErrorMessage = "Số điện thoại phải có 10 chữ số")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    [Compare(nameof(Password),
        ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; } = "";
}
