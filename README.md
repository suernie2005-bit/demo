# QLSanPham Web API - Kế hoạch thực hành Tuần 4

## 1. Giới thiệu

Đây là dự án thực hành Backend Developer Tuần 4, xây dựng REST API quản lý loại sản phẩm và sản phẩm bằng:

- C# và .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server 2022 cài trực tiếp trên Windows
- Swagger/OpenAPI
- xUnit cho kiểm thử

Dự án **không sử dụng Docker**. Database được tạo trên SQL Server của máy phát triển bằng EF Core Code First và migrations.

## 2. Mục tiêu Tuần 4

Sau khi hoàn thành, dự án phải đáp ứng:

- Tạo lại được database `QLSanphamDB` từ repository sạch.
- Có CRUD đầy đủ cho `Loai` và `SanPham`.
- Có tìm kiếm, lọc, sắp xếp và phân trang.
- Query đọc sử dụng async, projection DTO và `AsNoTracking()`.
- Input không hợp lệ trả `400 Bad Request`.
- Không tìm thấy dữ liệu trả `404 Not Found`.
- Dữ liệu trùng hoặc xung đột trả `409 Conflict`.
- Tạo thành công trả `201 Created`.
- Xóa thành công trả `204 No Content`.
- Có ít nhất 10 automated tests có ý nghĩa và chạy pass.
- Có migration, SQL schema, seed data, ERD và hướng dẫn chạy dự án.

## 3. Trạng thái hiện tại

Đã hoàn thành Mốc 1 trên máy phát triển:

- Chỉ còn một `QLSanPhamDBContext`, dùng Code First.
- Có migration `20261027072150_InitialCreate`.
- Database `QLSanphamDB` đã được tạo trên SQL Server 2022 Express tại `LAPTOP-G1L8V9TM`.
- Bảng `Loai`, `SanPham` và `__EFMigrationsHistory` đã được xác nhận.
- Mỗi bảng có 3 dòng seed data.
- Có ràng buộc FK, unique index và check constraint.
- Có schema SQL, seed SQL và ERD trong thư mục `Database`.
- Connection string mặc định dùng Windows Authentication tới SQL Server cục bộ; có thể override bằng User Secrets.

Các phần hiện có từ trước:

- Controller và service CRUD cho `Loai` và `SanPham`.
- DTO và Data Annotations validation cơ bản.
- Tìm kiếm, lọc, sắp xếp và phân trang sản phẩm.

Mốc 2 đã thêm global exception handler và chuẩn hóa lỗi API. Mốc 3 đã hoàn thiện kiểm tra phân trang, sắp xếp và tên sau khi chuẩn hóa. Mốc 4 có 31 service tests chạy độc lập bằng EF Core InMemory. Mốc 5 đã bổ sung ví dụ cho toàn bộ endpoint trong `WebAPI.http`, mô tả response trên Swagger và hướng dẫn build/chạy project. Chưa chạy migration hoặc CRUD trên SQL Server trong Mốc 5 để giữ nguyên database hiện tại theo yêu cầu.

## 4. Kiến trúc dữ liệu dự kiến

```text
Loai
├── Id          int, PK, Identity
├── Ten         nvarchar(100), NOT NULL, UNIQUE
└── Mota        nvarchar(255), NULL
    │
    │ 1
    │
    └───────────────< N
                      SanPham
                      ├── Id          int, PK, Identity
                      ├── TenSP       nvarchar(150), NOT NULL
                      ├── MoTaSP      nvarchar(500), NULL
                      ├── Gia         decimal(18,2), CHECK > 0
                      ├── SoLuong     int, CHECK >= 0
                      ├── LoaiId      int, FK -> Loai.Id
                      └── Ngay        datetime2, DEFAULT SYSUTCDATETIME()
```

Một sản phẩm thuộc đúng một loại. Một loại có thể có nhiều sản phẩm. Cặp `(TenSP, LoaiId)` phải duy nhất để không trùng tên sản phẩm trong cùng loại.

## 5. Phần người dùng cần chuẩn bị

### 5.1. SQL Server

Máy hiện tại có SQL Server 2022 Express và SQL Server Management Studio (SSMS):

- Server: `LAPTOP-G1L8V9TM`
- Engine version: `16.0.1000.6`
- Database `QLSanphamDB` đã được tạo bằng migration.
- Migrations đã được áp dụng bằng Windows Authentication.

Để áp dụng các migration mới về sau, cần bảo đảm:

1. SQL Server service đang chạy.
2. Có thể kết nối server cục bộ trong SSMS.
3. Tài khoản Windows đang chạy migration có quyền sửa database.
4. Nếu dùng SQL login, lưu connection string bằng User Secrets; không gửi mật khẩu qua chat và không commit vào Git.
5. Trên máy mới, migration chỉ tạo database sau khi bạn chủ động chạy lệnh áp migration ở mục 8; ứng dụng không tự chạy migration khi khởi động.

Kiểm tra kết nối Windows Authentication bằng PowerShell:

```powershell
sqlcmd -S "localhost" -E -Q "SELECT @@SERVERNAME AS ServerName, @@VERSION AS Version;"
```

### 5.2. .NET SDK

Kiểm tra SDK:

```powershell
dotnet --version
```

Máy hiện tại có .NET SDK `9.0.319`; SDK này có thể build project target `net8.0`. Nếu build báo thiếu .NET 8 targeting/runtime pack, cài thêm .NET 8 SDK.

### 5.3. Kết nối NuGet

Cần cho phép truy cập NuGet trong lần đầu để:

- Restore các package của project.
- Cài `dotnet-ef` phiên bản 8 dưới dạng local tool.
- Restore package cho project test.

### 5.4. Connection string và mật khẩu

Connection string Windows Authentication mặc định trong `appsettings.json`:

```text
Server=localhost;Database=QLSanphamDB;Trusted_Connection=True;TrustServerCertificate=True;
```

Nếu cần dùng SQL Authentication, connection string có mật khẩu phải được lưu bằng .NET User Secrets, không lưu trong `appsettings.json`.

Sau khi phần User Secrets được cấu hình trong project, người dùng chạy tại thư mục `WebAPI`:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=LAPTOP-G1L8V9TM;Database=QLSanphamDB;User Id=sa;Password=<YOUR_PASSWORD>;TrustServerCertificate=True;" --project .\WebAPI\WebAPI.csproj
```

Thay `<YOUR_PASSWORD>` trên máy cá nhân. Không gửi hoặc commit câu lệnh đã chứa mật khẩu thật.

## 6. Phần Codex sẽ triển khai

### Mốc 1 - Chuẩn hóa database

- [x] Chọn Code First làm quy trình chính.
- [x] Chỉ giữ một `QLSanPhamDBContext`.
- [x] Loại bỏ connection string và mật khẩu cũ trong source.
- [x] Thêm FK, unique index và check constraints.
- [x] Tạo migration `InitialCreate` và seed data.
- [x] Xuất `Database/schema.sql` và `Database/seed.sql`.
- [x] Tạo ERD tại `Database/ERD.md`.
- [x] Áp migration và xác minh trực tiếp trên SQL Server.

### Mốc 2 - Hoàn thiện REST API

- [x] CRUD cho `Loai` và `SanPham`.
- [x] Thêm global exception handler.
- [x] Trả response lỗi theo `ProblemDetails`.
- [x] Ánh xạ lỗi nghiệp vụ thành `400`, `404` và `409`.
- [x] Giữ `201` khi tạo và `204` khi xóa thành công.
- [x] Gỡ endpoint mẫu `WeatherForecast`.

### Mốc 3 - Truy vấn và validation

- [x] Giới hạn `pageSize` từ 1 đến 100.
- [x] Chỉ cho phép các trường sắp xếp đã định nghĩa.
- [x] Chỉ chấp nhận `sortOrder=asc` hoặc `sortOrder=desc`.
- [x] Tìm kiếm sản phẩm theo tên và lọc theo `LoaiId`.
- [x] Dùng async query, projection và `AsNoTracking()` cho truy vấn đọc.
- [x] Từ chối tên rỗng hoặc chỉ gồm khoảng trắng sau khi trim.
- [x] Ngăn dữ liệu trùng ở service và bằng unique index trong database.

### Mốc 4 - Kiểm thử

Đã tạo 31 service tests trong `WebAPI.Tests`, dùng database InMemory riêng cho từng test:

- [x] Page và page size không hợp lệ.
- [x] Trường hoặc chiều sắp xếp không hợp lệ.
- [x] Bản ghi không tồn tại.
- [x] Tạo loại trùng tên.
- [x] Xóa loại đang có sản phẩm bị từ chối.
- [x] Tạo sản phẩm với loại không tồn tại.
- [x] Tạo sản phẩm trùng trong cùng loại.
- [x] Cập nhật bản ghi không tồn tại.
- [x] Xóa sản phẩm không tồn tại.
- [x] Trang vượt dữ liệu trả danh sách rỗng.
- [x] Các luồng CRUD thành công.

### Mốc 5 - Tài liệu và nghiệm thu

- [x] Hoàn thiện `WebAPI.http` cho GET, POST, PUT, DELETE, tìm kiếm/lọc, phân trang và các lỗi thường gặp. Các request ghi dữ liệu có cảnh báo ngay trong file.
- [x] Bổ sung response types `200`, `201`, `204`, `400`, `404`, `409` và `500` cho Swagger.
- [x] Hoàn thiện README; migration, schema, seed và ERD đã có từ Mốc 1.
- [ ] Chạy lại migration/CRUD trên một database SQL Server mới hoàn toàn. Chưa thực hiện để không thay đổi database đang có.
- [x] Build solution và chạy toàn bộ test không sử dụng SQL Server.

## 7. Cấu trúc thư mục mục tiêu

```text
demo-main/
├── README.md
├── demo-main.sln
├── dotnet-tools.json
├── Database/
│   ├── schema.sql
│   ├── seed.sql
│   └── ERD.md
├── WebAPI/
│   ├── Controllers/
│   ├── Data/
│   │   └── Migrations/
│   ├── DTOs/
│   ├── Exceptions/
│   ├── Models/
│   ├── Services/
│   ├── Program.cs
│   ├── WebAPI.csproj
│   └── WebAPI.http
└── WebAPI.Tests/
    ├── Services/
    └── WebAPI.Tests.csproj
```

## 8. Cấu hình kết nối và dựng database

Mặc định ứng dụng dùng Windows Authentication tới SQL Server cục bộ (`localhost`); đó cũng là cách đã được dùng để dựng database trên máy phát triển này. Nếu máy của bạn chỉ cho phép login `sa`, lưu connection string vào User Secrets. Lệnh `user-secrets set` là tùy chọn. Chạy trong PowerShell từ thư mục repository và thay `<YOUR_PASSWORD>` bằng mật khẩu trên máy cá nhân. Không chia sẻ mật khẩu qua chat hoặc commit vào repository.

Để kiểm tra khả năng dựng mới, hãy trỏ User Secrets tới một database thử nghiệm riêng, ví dụ `QLSanphamDB_Moc5_Validation`. Lệnh `database update` tạo/cập nhật schema và seed dữ liệu; không chạy lệnh này nếu connection string vẫn trỏ tới database đang chứa dữ liệu cần giữ nguyên.

```powershell
cd D:\ThucTap\demo-main
dotnet tool restore
dotnet restore .\WebAPI\WebAPI.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=QLSanphamDB_Moc5_Validation;Trusted_Connection=True;TrustServerCertificate=True;" --project .\WebAPI\WebAPI.csproj
dotnet ef database update `
  --project .\WebAPI\WebAPI.csproj `
  --startup-project .\WebAPI\WebAPI.csproj `
  --context WebAPI.Data.QLSanPhamDBContext
```

Trước khi áp migration, xác nhận trong SSMS rằng connection string đang trỏ đúng database thử nghiệm riêng. Không dùng `QLSanphamDB` nếu cần giữ nguyên database hiện tại.

Nếu chọn SQL Authentication, chạy thêm lệnh này một lần trước khi chạy API. Lệnh lưu giá trị cục bộ trong User Secrets, không ghi vào repository:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=LAPTOP-G1L8V9TM;Database=QLSanphamDB;User Id=sa;Password=<YOUR_PASSWORD>;TrustServerCertificate=True;" --project .\WebAPI\WebAPI.csproj
```

`dotnet-tools.json` ghim `dotnet-ef` phiên bản 8.0.0 để đồng bộ major version với EF Core packages. Migration dùng `HasData`; do đó database được tạo mới sẽ tự có các bản ghi mẫu. `Database/seed.sql` là script riêng, chạy sau schema khi cần nạp lại dữ liệu mẫu. Dùng schema/seed SQL cũng có thể thay đổi dữ liệu nếu chạy trên database đang sử dụng; chỉ áp dụng trên database thử nghiệm.

Để tạo lại script schema từ migration hiện tại:

```powershell
dotnet ef migrations script 0 InitialCreate `
  --project .\WebAPI\WebAPI.csproj `
  --startup-project .\WebAPI\WebAPI.csproj `
  --context WebAPI.Data.QLSanPhamDBContext `
  --output .\Database\schema.sql
```

Kiểm tra database:

```powershell
sqlcmd -S "localhost" -E -d QLSanphamDB -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';"
```

Trong SSMS, refresh mục `Databases`. Database `QLSanphamDB` phải xuất hiện cùng các bảng:

- `Loai`
- `SanPham`
- `__EFMigrationsHistory`

## 9. Chạy ứng dụng

### Visual Studio

1. Chọn **File → Open → Project or Solution**.
2. Mở `D:\ThucTap\demo-main\demo-main.sln` (mở thư mục hoặc chỉ mở `WebAPI.csproj` sẽ không có lệnh **Build Solution**).
3. Trong Solution Explorer, nhấp phải project `WebAPI` → **Set as Startup Project**.
4. Trên thanh chạy, chọn profile `https`.
5. Chọn **Build → Build Solution** hoặc nhấn `Ctrl+Shift+B` để biên dịch; nhấn `F5` để chạy API.

Build Solution chỉ biên dịch Web API và project tests; nó không chạy test và không kết nối SQL Server. Khi chạy API, ứng dụng dùng SQL Server cục bộ đã cấu hình. Để giữ dữ liệu hiện tại, chỉ gọi các API `GET`; các API `POST`, `PUT`, `DELETE` sẽ thay đổi dữ liệu.

### Terminal

```powershell
cd D:\ThucTap\demo-main\WebAPI
dotnet restore
dotnet run --launch-profile https
```

Để build cả solution từ terminal:

```powershell
cd D:\ThucTap\demo-main
dotnet build .\demo-main.sln
```

Địa chỉ dự kiến:

- Swagger: `https://localhost:7266/swagger`
- HTTP: `http://localhost:5053`

Nếu máy chưa tin cậy HTTPS development certificate:

```powershell
dotnet dev-certs https --trust
```

## 10. API dự kiến

### Loại sản phẩm

| Method | Endpoint | Chức năng |
|---|---|---|
| `GET` | `/api/loai` | Danh sách loại có phân trang và sắp xếp |
| `GET` | `/api/loai/{id}` | Lấy loại theo ID |
| `POST` | `/api/loai` | Tạo loại |
| `PUT` | `/api/loai/{id}` | Cập nhật loại |
| `DELETE` | `/api/loai/{id}` | Xóa loại |

### Sản phẩm

| Method | Endpoint | Chức năng |
|---|---|---|
| `GET` | `/api/sanpham` | Danh sách sản phẩm có tìm kiếm, lọc, sắp xếp và phân trang |
| `GET` | `/api/sanpham/{id}` | Lấy sản phẩm theo ID |
| `POST` | `/api/sanpham` | Tạo sản phẩm |
| `PUT` | `/api/sanpham/{id}` | Cập nhật sản phẩm |
| `DELETE` | `/api/sanpham/{id}` | Xóa sản phẩm |

Tham số danh sách:

- `/api/loai`: `page` (từ 1), `pageSize` (1-100), `sortBy` (`id` hoặc `ten`), `sortOrder` (`asc` hoặc `desc`).
- `/api/sanpham`: thêm `search` để tìm theo tên và `loaiId` (số nguyên dương) để lọc; `sortBy` nhận `id`, `tenSP`, `gia`, `soLuong` hoặc `ngay`.
- Nếu `sortBy` hoặc `sortOrder` không nằm trong danh sách hợp lệ, API trả `400 Bad Request`.
- Tên loại và tên sản phẩm được trim trước khi lưu; tên rỗng hoặc chỉ có khoảng trắng trả `400`.
- Tên loại trùng hoặc tên sản phẩm trùng trong cùng loại trả `409`; unique indexes trong SQL Server vẫn bảo vệ dữ liệu khi có request đồng thời.

## 11. Chạy kiểm thử

Chạy từ thư mục repository:

```powershell
cd D:\ThucTap\demo-main
dotnet test .\WebAPI.Tests\WebAPI.Tests.csproj
```

Test dùng EF Core InMemory với tên database riêng cho từng test; không cần chạy SQL Server và không đọc/ghi database phát triển.

Điều kiện nghiệm thu:

- Build thành công, không có compilation error.
- Tất cả test pass.
- Test không phụ thuộc database đang chạy hoặc network bên ngoài.
- API kết nối được SQL Server thật khi chạy ứng dụng.
- CRUD và status code đúng yêu cầu.
- Trang không có dữ liệu trả `200 OK` với `items` rỗng.

## 12. Quy tắc bảo mật và source control

- Không commit mật khẩu, connection string có mật khẩu hoặc file backup chứa dữ liệu nhạy cảm.
- Không sử dụng tài khoản `sa` cho production.
- `sa` chỉ được dùng tạm thời cho môi trường học tập cục bộ.
- Connection string có mật khẩu phải được lưu trong User Secrets.
- Không đưa thư mục `bin/`, `obj/`, `.vs/`, file `.user` hoặc test results vào source control.
- Mọi thay đổi schema phải đi qua migration hoặc SQL script có kiểm soát.

## 13. Xử lý lỗi thường gặp

### Không kết nối được SQL Server

- Kiểm tra SQL Server service đang chạy.
- Kiểm tra đúng server `LAPTOP-G1L8V9TM`.
- Kiểm tra lại chế độ SQL Server Authentication và mật khẩu `sa`.
- Kiểm tra kết nối trực tiếp trong SSMS trước khi chạy API.

### `Login failed for user 'sa'`

- Kiểm tra server đã bật Mixed Mode Authentication.
- Kiểm tra login `sa` đang enabled.
- Không thử nhiều lần bằng mật khẩu không chắc chắn để tránh khóa hoặc gây nhầm lẫn cấu hình.

### Không tìm thấy `dotnet ef`

Chạy tại repository sau khi local tool manifest được thêm:

```powershell
dotnet tool restore
dotnet ef --version
```

### Database chưa xuất hiện trong SSMS

- Chỉ chạy `dotnet ef database update` theo hướng dẫn ở mục 8 sau khi xác nhận connection string trỏ tới database thử nghiệm riêng; không chạy trên database đang cần bảo toàn.
- Refresh mục `Databases` trong Object Explorer.
- Kiểm tra application đang dùng đúng connection string và server.

## 14. Checklist các mốc đã hoàn thành

- [x] Kết nối được SQL Server `LAPTOP-G1L8V9TM`.
- [x] Tạo database `QLSanphamDB`.
- [x] Tạo bảng `Loai`, `SanPham` và `__EFMigrationsHistory`.
- [x] Áp dụng migration `20261027072150_InitialCreate`.
- [x] Xác nhận 3 loại và 3 sản phẩm mẫu.
- [x] Xác nhận hai unique indexes, foreign key và hai check constraints.
- [x] Chạy lại `Database/seed.sql` mà không sinh bản ghi trùng.
- [x] Build project Web API thành công.
- [x] Mốc 2: global exception handler, status code và gỡ WeatherForecast.
- [x] Mốc 3: validation, sort, search, filter và phân trang.
- [x] Mốc 4: 31 service tests pass.
- [x] Mốc 5: request samples, Swagger response docs, README và solution đã hoàn thiện.
- [ ] Xác minh migration và CRUD trên database SQL Server mới: chưa thực hiện theo yêu cầu không thay đổi database.

Mốc 1 đã được áp dụng lên SQL Server cục bộ. Mốc 2-5 hoàn tất trong source code, README và các artifact. Test Mốc 4 dùng InMemory nên không thay đổi database SQL Server. Mốc 5 chưa chạy API hoặc migration trên SQL Server; các request POST/PUT/DELETE trong `WebAPI.http` có thể ghi dữ liệu khi chạy.
