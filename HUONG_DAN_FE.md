# Giao diện RENTO (Blazor) – các trang mới

Thư mục `CarRentalFE/` trong gói này có cùng cấu trúc với project. Copy **đè vào** thư mục `CarRentalFE` trong repo của bạn
(chỉ thêm file mới, không ghi đè file có sẵn). Sau đó sửa 3 file chung theo hướng dẫn bên dưới.

## File mới
| File | Mô tả |
|---|---|
| `wwwroot/public.css` | CSS giao diện khách hàng (từ Figma), mọi selector nằm trong `.pub` nên không ảnh hưởng trang khác |
| `wwwroot/admin/vehicles.css` | CSS trang quản lý xe (admin), dùng chung màu với dashboard |
| `Models/CarDtos.cs` | Các lớp dữ liệu khớp JSON của API |
| `Services/CarApiService.cs` | Gọi API xe / hãng / loại / dashboard của BE |
| `Components/Shared/Icon.razor`, `CarCard.razor` | Icon SVG và thẻ xe dùng lại nhiều nơi |
| `Components/Layout/PublicLayout.razor` | Header + footer của giao diện khách |
| `Components/Pages/public/Home.razor` | Trang chủ `/` |
| `Components/Pages/public/CarSearch.razor` | Tìm / lọc xe `/cars` |
| `Components/Pages/public/CarDetail.razor` | Chi tiết xe `/cars/{id}` |
| `Components/Pages/admin/Vehicles.razor` | Quản lý xe, loại xe, hãng xe `/vehicles` |

## Chạy thử
1. Chạy backend: `dotnet run --launch-profile http` trong `CarRentalAPI` (cổng 5110), đã nhập `seed.sql`.
2. Chạy frontend: `dotnet run --launch-profile http` trong `CarRentalFE` (cổng 5088).
3. Mở `http://localhost:5088/` (trang chủ), `/cars` (tìm xe), `/vehicles` (admin).

## Còn thiếu / cần lưu ý
- **Chưa biên dịch thử** (môi trường tạo gói này không có .NET). Nếu `dotnet build` báo lỗi, gửi lại thông báo.
- **Ảnh xe**: `seed.sql` chỉ có đường dẫn giả (`/images/cars/...`), nên thẻ xe hiện biểu tượng xe thay cho ảnh. Thêm ảnh thật (link `https://...`) qua form sửa xe ở `/vehicles`.
- **Nút "Đặt xe"** ở trang chi tiết dẫn tới `/checkout`, trang này chưa có (thuộc luồng đặt xe của BE1/FE1).
- **Liên kết `/login`** ở header cũng chưa có trang.
- **`AuthService`** có sẵn trong repo đang gọi `http://localhost:5000/api/v1/`, không khớp backend (`http://localhost:5110/`, không có `/api/v1`). Cần FE/BE1 thống nhất trước khi làm đăng ký / đăng nhập.
- Trang admin chưa đăng nhập / phân quyền vì BE chưa có JWT.
- Chưa làm: Rentals, BookingDetails, Checkout, Confirmation, Login, và các màn admin Orders / Returns / Finance / Support / Reviews / Customers / Settings.
