# ERD - QLSanphamDB

```mermaid
erDiagram
    Loai ||--o{ SanPham : "phan loai"

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
```

Ràng buộc:

- `Loai.Ten` là duy nhất.
- Cặp `SanPham(LoaiId, TenSP)` là duy nhất.
- `SanPham.LoaiId` tham chiếu `Loai.Id`; không xóa loại nếu còn sản phẩm.
- `SanPham.Gia > 0`.
- `SanPham.SoLuong >= 0`.
- `SanPham.Ngay` mặc định dùng UTC từ SQL Server.
