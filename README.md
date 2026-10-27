# QLSanPham Web API — Tuần 5

Web API quản lý loại sản phẩm và sản phẩm, tổ chức theo Clean Architecture, CQRS/MediatR và ASP.NET Core Identity. Tuần 5 bổ sung validation pipeline, transaction, JWT, refresh token rotation, role authorization và kiểm thử tự động.

## Trạng thái

- Solution build thành công: **0 warning, 0 error**.
- Test gần nhất: **77 passed, 0 failed, 3 SQL integration tests skipped**.
- Migration Identity và refresh token đã có trong source nhưng chưa áp dụng lên SQL Server.
- Không tự động chạy migration khi API khởi động.
- Database hiện có `QLSanphamDB` phải được giữ nguyên.

## Công nghệ

- .NET 8 và ASP.NET Core Web API
- Entity Framework Core 8 và SQL Server
- ASP.NET Core Identity
- MediatR và FluentValidation
- JWT Bearer Authentication
- xUnit, EF Core InMemory và ASP.NET TestServer
- Swagger/OpenAPI

## Kiến trúc

```text
demo-main/
├── Domain/          # Entity và quy tắc miền
├── Application/     # Command, query, handler, validator và abstraction
├── Infrastructure/  # EF Core, SQL Server, Identity, JWT và repository
├── WebAPI/          # Controller, middleware và dependency injection
├── WebAPI.Tests/    # Unit, HTTP và SQL integration tests
└── Database/        # Migration script, ERD và script cấp role Admin
```

Hướng phụ thuộc:

```text
Domain ← Application ← Infrastructure
              ↑              ↑
              └── WebAPI ────┘
```

API chỉ sử dụng một `QLSanPhamDBContext`, đặt trong Infrastructure.

## Nội dung đã triển khai

| Mốc | Nội dung | Trạng thái |
|---|---|---|
| 1 | Tách Domain, Application, Infrastructure, WebAPI và Tests | Hoàn thành |
| 2 | CRUD `Loai`, `SanPham` qua CQRS/MediatR | Hoàn thành |
| 3 | FluentValidation và validation pipeline | Hoàn thành |
| 4 | Transaction pipeline và rollback | Unit test pass; SQL test chưa chạy |
| 5 | Identity, refresh-token persistence và migration | Có trong source; chưa áp migration |
| 6 | Register, login, JWT, refresh rotation, logout và role Admin | Hoàn thành trong source |
| 7 | Unit/HTTP tests, Swagger Bearer, WebAPI.http, schema và ERD | 77 test pass; 3 SQL test skip |

## API chính

### Xác thực

| Method | Endpoint | Quyền |
|---|---|---|
| POST | `/api/auth/register` | Công khai |
| POST | `/api/auth/login` | Công khai |
| POST | `/api/auth/refresh` | Công khai, yêu cầu refresh token |
| POST | `/api/auth/logout` | Công khai, yêu cầu refresh token |

Đăng ký luôn gán role `User`. Role `Admin` chỉ được cấp thủ công trong database thử nghiệm bằng [PromoteAdmin.sql](Database/PromoteAdmin.sql).

### Danh mục và sản phẩm

| Method | Endpoint | Quyền |
|---|---|---|
| GET | `/api/loai` | Công khai |
| GET | `/api/loai/{id}` | Công khai |
| POST/PUT/DELETE | `/api/loai` | Admin |
| GET | `/api/sanpham` | Công khai |
| GET | `/api/sanpham/{id}` | Công khai |
| POST/PUT/DELETE | `/api/sanpham` | Admin |

Danh sách hỗ trợ phân trang, sắp xếp; sản phẩm hỗ trợ tìm kiếm theo tên và lọc theo loại. `pageSize` hợp lệ từ 1 đến 100.

## Cấu hình máy phát triển

Chạy các lệnh tại `D:\ThucTap\demo-main`.

### 1. Khôi phục công cụ và package

```powershell
dotnet tool restore
dotnet restore .\demo-main.sln
```

### 2. Tạo JWT signing key

```powershell
$jwtKey = [Convert]::ToBase64String(
    [Security.Cryptography.RandomNumberGenerator]::GetBytes(64))

dotnet user-secrets set "Jwt:SigningKey" $jwtKey `
    --project .\WebAPI\WebAPI.csproj

Remove-Variable jwtKey
```

Không commit signing key, mật khẩu hoặc connection string có credential.

### 3. Cấu hình database thử nghiệm

Chỉ sử dụng database riêng `QLSanphamDB_Week5_Test` cho migration và request ghi dữ liệu:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" `
    "Server=localhost;Database=QLSanphamDB_Week5_Test;Trusted_Connection=True;TrustServerCertificate=True;" `
    --project .\WebAPI\WebAPI.csproj
```

> Cấu hình source hiện vẫn có fallback tới `QLSanphamDB`. Phải kiểm tra User Secrets trước khi chạy migration hoặc request `POST`, `PUT`, `DELETE`.

### 4. Tạo database thử nghiệm khi thực sự cần

Lệnh sau sẽ tạo hoặc cập nhật schema SQL Server. Chỉ chạy sau khi xác nhận connection string đang trỏ đúng `QLSanphamDB_Week5_Test`:

```powershell
dotnet ef database update `
    --project .\Infrastructure\Infrastructure.csproj `
    --startup-project .\Infrastructure\Infrastructure.csproj `
    --context QLSanPham.Infrastructure.Persistence.QLSanPhamDBContext
```

Schema đầy đủ có thể xem tại [Database/schema.sql](Database/schema.sql), ERD tại [Database/ERD.md](Database/ERD.md).

## Build và chạy API

```powershell
dotnet build .\demo-main.sln
dotnet run --project .\WebAPI\WebAPI.csproj
```

Mặc định:

- HTTP: `http://localhost:5053`
- HTTPS: `https://localhost:7266`
- Swagger: `/swagger`

Các request mẫu nằm trong [WebAPI/WebAPI.http](WebAPI/WebAPI.http). Không gửi request ghi dữ liệu khi API đang trỏ tới `QLSanphamDB`.

## Chạy test không dùng SQL Server

```powershell
Remove-Item Env:QLSANPHAM_SQLSERVER_TEST_CONNECTION -ErrorAction SilentlyContinue
dotnet test .\WebAPI.Tests\WebAPI.Tests.csproj
```

Kết quả gần nhất:

```text
Passed:  77
Failed:   0
Skipped:  3 SQL integration tests
Total:   80
```

Các test thường dùng EF Core InMemory và TestServer, không truy cập SQL Server.

## Chạy SQL integration tests

Các test này chỉ chấp nhận database có tên chính xác `QLSanphamDB_Week5_Test`:

```powershell
$env:QLSANPHAM_SQLSERVER_TEST_CONNECTION = `
    "Server=localhost;Database=QLSanphamDB_Week5_Test;Trusted_Connection=True;TrustServerCertificate=True;"

dotnet test .\WebAPI.Tests\WebAPI.Tests.csproj `
    --filter FullyQualifiedName~WebAPI.Tests.Integration
```

Không đặt biến môi trường trên nếu chưa chuẩn bị database thử nghiệm. Không dùng `QLSanphamDB` cho integration test.

## Quy tắc HTTP

- `200`: đọc hoặc cập nhật thành công
- `201`: tạo thành công
- `204`: xóa hoặc logout thành công
- `400`: input hoặc validation không hợp lệ
- `401`: thiếu/sai/hết hạn token hoặc đăng nhập thất bại
- `403`: đã xác thực nhưng không có role Admin
- `404`: không tìm thấy dữ liệu
- `409`: dữ liệu trùng hoặc xung đột nghiệp vụ

Lỗi API được trả theo ProblemDetails và có `traceId`.

## Việc còn lại trước nghiệm thu SQL

- Đổi cấu hình mặc định để không còn fallback vào `QLSanphamDB`.
- Bổ sung kiểm soát cạnh tranh khi hai request cùng refresh một token.
- Chạy 3 SQL integration tests trên `QLSanphamDB_Week5_Test`.
- Xác minh trực tiếp bảng Identity, `RefreshToken`, foreign key, unique index và check constraint.
- Nếu yêu cầu bỏ hoàn toàn tiền tố `QLSanPham.*`, cần đổi tiếp assembly name và namespace; hiện mới đổi tên thư mục/project hiển thị.

## Ngoài phạm vi Tuần 5

MongoDB, RabbitMQ, gRPC, outbox, background worker và Docker chưa được triển khai.
