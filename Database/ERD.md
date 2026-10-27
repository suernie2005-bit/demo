# ERD — QLSanphamDB

~~~mermaid
erDiagram
    Loai ||--o{ SanPham : "phan loai"

    AspNetUsers ||--o{ RefreshToken : "so huu"
    RefreshToken o|--o| RefreshToken : "duoc thay the boi"

    AspNetUsers ||--o{ AspNetUserRoles : "duoc gan"
    AspNetRoles ||--o{ AspNetUserRoles : "gan cho user"

    Loai {
        int Id PK
        nvarchar_100 Ten UK
        nvarchar_255 Mota
    }

    SanPham {
        int Id PK
        nvarchar_150 TenSP
        nvarchar_500 MoTaSP
        decimal_18_2 Gia
        int SoLuong
        int LoaiId FK
        datetime2 Ngay
    }

    AspNetUsers {
        nvarchar_450 Id PK
        nvarchar_256 Email UK
        nvarchar_256 NormalizedEmail UK
        nvarchar_max PasswordHash
    }

    AspNetRoles {
        nvarchar_450 Id PK
        nvarchar_256 Name
        nvarchar_256 NormalizedName UK
    }

    AspNetUserRoles {
        nvarchar_450 UserId PK, FK
        nvarchar_450 RoleId PK, FK
    }

    RefreshToken {
        uniqueidentifier Id PK
        nvarchar_450 UserId FK
        char_64 TokenHash UK
        uniqueidentifier FamilyId
        datetime2 CreatedAtUtc
        datetime2 ExpiresAtUtc
        datetime2 RevokedAtUtc
        uniqueidentifier ReplacedByTokenId FK, UK
    }
~~~

Ràng buộc catalog:

- Loai.Ten là duy nhất.
- Cặp SanPham(LoaiId, TenSP) là duy nhất.
- SanPham.LoaiId tham chiếu Loai.Id; không xóa loại nếu còn sản phẩm.
- SanPham.Gia > 0.
- SanPham.SoLuong >= 0.
- SanPham.Ngay mặc định dùng UTC từ SQL Server.

Ràng buộc xác thực:

- Email chuẩn hóa của user là duy nhất; PasswordHash do ASP.NET Identity quản lý.
- Role được gán qua bảng nối AspNetUserRoles.
- RefreshToken.TokenHash là SHA-256 dạng hex 64 ký tự và duy nhất; raw refresh token không lưu ở database.
- Refresh token phải hết hạn sau thời điểm tạo; thời điểm thu hồi không trước thời điểm tạo.
- RefreshToken.ReplacedByTokenId trỏ tới refresh token thay thế. Các chuỗi refresh được gom theo FamilyId.
