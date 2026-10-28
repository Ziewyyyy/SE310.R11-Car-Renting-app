# Car Rental System — Database Seed Data

Bộ dữ liệu mẫu dành cho nhóm SE310.R11 — Car Renting App.

## Nội dung

- `database/seed.sql`: **153 bản ghi giả lập**, phủ **20/21 bảng nghiệp vụ**; bảng `RefreshTokens` để trống vì bảo mật.
- Dữ liệu gồm 8 tài khoản mẫu (1 Admin, 2 Staff, 5 Customer), 7 hãng xe, 5 loại xe, 12 xe, 12 ảnh, 8 tiện ích, các đơn thuê, hợp đồng, thanh toán, đánh giá, hỗ trợ và các bảng còn lại.
- File SQL dùng tên bảng/cột **PascalCase, có dấu ngoặc kép**, khớp với bộ Entity và `AppDbContext` đã chia sẻ trước đó.

> **Quan trọng:** Đây là dữ liệu **giả lập phục vụ phát triển/demo**, không phải dữ liệu thật. Biển số, số điện thoại, giao dịch VNPay, ảnh đều là placeholder. Không sử dụng để báo cáo doanh thu thực tế.

## A. Mỗi thành viên tạo database riêng

### 1. Clone project

```powershell
git clone <URL_REPOSITORY_CUA_NHOM>
cd SE310.R11-Car-Renting-app/CarRentalAPI
dotnet restore
```

Nếu đã clone thì dùng `git pull`.

### 2. Cài PostgreSQL và cấu hình kết nối

Cài PostgreSQL trên máy. Ví dụ dùng database `CarRentalDB`, host `localhost`, port `5432`.

Không commit mật khẩu vào Git. Với ASP.NET Core, đặt connection string cục bộ bằng **User Secrets**:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CarRentalDB;Username=postgres;Password=MAT_KHAU_LOCAL"
```

**Lưu ý:** `dotnet ef` sử dụng cấu hình project. Nếu có `ConnectionStrings__DefaultConnection` trong biến môi trường hoặc `appsettings.Development.json`, kiểm tra để không trỏ nhầm server.

### 3. Tạo 22 bảng từ Migration

Đứng trong thư mục chứa `CarRentalAPI.csproj`:

```powershell
dotnet ef database update
```

Nếu chưa có công cụ EF Core CLI, cài phiên bản tương thích .NET 9:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.2
```

Chỉ chạy bước nhập dữ liệu khi đã có cả `InitialCreate` và `AddRemainingTables` và nhìn thấy 21 bảng nghiệp vụ + `__EFMigrationsHistory`.

### 4. Nhập dữ liệu mẫu bằng pgAdmin

1. Mở pgAdmin, chọn **đúng server local** mà Backend đang kết nối.
2. Mở `Databases → CarRentalDB`.
3. Nhấn chuột phải `CarRentalDB` → **Query Tool**.
4. Mở file `database/seed.sql` (nút Open File) hoặc copy toàn bộ nội dung vào Query Tool.
5. Nhấn **Execute (F5)**.
6. Kiểm tra cuối file: `Users`, `Cars`, `Bookings`, `Payments`, `CarImages` có số dòng > 0.

**Quan trọng:** Không chạy `seed.sql` trên database `postgres` mặc định, không chạy trên database cloud/production.

### 5. Nhập bằng psql (tùy chọn)

Chạy từ **thư mục gốc repository**:

```powershell
psql -h localhost -p 5432 -U postgres -d CarRentalDB -v ON_ERROR_STOP=1 -f .\database\seed.sql
```

Nếu file nằm trong `CarRentalAPI/database/seed.sql` thì điều chỉnh đường dẫn. Có thể dùng pgAdmin nếu chưa cài `psql` vào PATH.

## B. Kiểm tra kết quả

```sql
SELECT COUNT(*) FROM "Cars";
SELECT COUNT(*) FROM "Users";
SELECT COUNT(*) FROM "Bookings";
SELECT COUNT(*) FROM "CarBrands";
SELECT COUNT(*) FROM "CarCategories";
```

Trên database trống, sau khi seed:
- `Cars`: 12
- `Users`: 8
- `Bookings`: 8
- `CarBrands`: 7
- `CarCategories`: 5

Các bảng khác có dữ liệu mẫu; `RefreshTokens` được cố ý để trống.

## C. Có chạy lại được không?

**Có.** File dùng UUID cố định và `ON CONFLICT DO NOTHING`, nên chạy lại không tạo trùng bản ghi cùng ID.

Tuy nhiên, nếu một thành viên đã thay đổi/xóa dữ liệu demo hoặc thêm bản ghi trùng email/biển số, `ON CONFLICT DO NOTHING` có thể bỏ qua một số dòng; khi đó số lượng thực tế khác kết quả mẫu. Script **không ghi đè dữ liệu đã có**.

Nếu có lỗi SQL, toàn bộ thao tác trong `BEGIN ... COMMIT` sẽ rollback. Xem lỗi đầu tiên và đối chiếu Migration.

## D. Đăng nhập tài khoản mẫu

**Không thể đăng nhập bằng các tài khoản seed hiện tại.** Trường `PasswordHash` chứa giá trị cố ý không hợp lệ `!DEMO_ACCOUNT_LOGIN_DISABLED!`; không có mật khẩu mặc định để tránh chia sẻ mật khẩu cứng trong GitHub.

Muốn demo chức năng Login, Backend phải tạo tài khoản qua API đăng ký/seed C# sử dụng **password hasher thật của hệ thống**, rồi phân quyền Admin/Staff theo cách an toàn. Không thay `PasswordHash` bằng mật khẩu dạng plaintext.

Các email mẫu dùng miền `.test` để tránh nhầm với tài khoản thật.

## E. Hình ảnh xe

`CarImages.Url` chứa đường dẫn **placeholder** dạng `/images/cars/vios.jpg`. Bộ ZIP không có file ảnh thật, nên Frontend cần ảnh mặc định hoặc thêm ảnh vào `wwwroot/images/cars/` và cập nhật URL.

## F. Quy ước nhóm

- Mỗi thành viên dùng database PostgreSQL **riêng**, chỉ chia sẻ **Entities, AppDbContext, Migrations và seed.sql** qua Git.
- Không commit `appsettings.json` có mật khẩu thật, `.env`, backup database thật hoặc token.
- Khi thêm/sửa Entity, một Backend phụ trách tạo Migration và commit cả migration + model snapshot; thành viên khác `git pull` rồi `dotnet ef database update`.
- Không sửa bảng trực tiếp bằng pgAdmin nếu nhóm đang dùng EF Core Code First.
- `Bookings.CarId` và `BookingDetails.CarId` cùng tồn tại để tương thích cấu trúc hiện tại; khi xây API cần quy định hai giá trị nhất quán.
- Ngày giờ trong seed là UTC (`timestamptz`); API nên chuyển sang múi giờ Việt Nam khi hiển thị.
- `Payment.TransactionCode` là mã giả `DEMO-VNPAY-*`, không phải thanh toán VNPay thật.

## G. Commit lên GitHub

Từ thư mục gốc repository, đặt `database/seed.sql` và `README_DATABASE.md` trong repository, rồi chạy:

```powershell
git add database/seed.sql README_DATABASE.md
git commit -m "chore: add PostgreSQL demo seed data and setup guide"
git push
```

Nếu repository đặt database trong `CarRentalAPI/database/`, sửa đường dẫn `git add` cho phù hợp.

## H. Lỗi thường gặp

- **relation does not exist**: chưa chạy Migration, chạy sai database, hoặc tên bảng khác schema.
- **duplicate key**: dữ liệu đã có; seed thường bỏ qua trùng qua `ON CONFLICT DO NOTHING`.
- **password authentication failed**: sai tài khoản/mật khẩu PostgreSQL.
- **connection refused**: PostgreSQL chưa chạy hoặc sai host/port.
- **đã seed nhưng không thấy dữ liệu**: kiểm tra server `local` vs `PostgreSQL 18` trong pgAdmin; chọn đúng database được Backend sử dụng.
