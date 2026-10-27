-- Seed data mẫu cho schema QLSanphamDB.
-- Migration InitialCreate đã chèn cùng các bản ghi này.
-- Các kiểm tra theo ID giúp script có thể chạy lại trên database hiện có.

SET IDENTITY_INSERT [dbo].[Loai] ON;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Loai] WHERE [Id] = 1)
    INSERT INTO [dbo].[Loai] ([Id], [Ten], [Mota])
    VALUES (1, N'Điện tử', N'Thiết bị và phụ kiện điện tử');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Loai] WHERE [Id] = 2)
    INSERT INTO [dbo].[Loai] ([Id], [Ten], [Mota])
    VALUES (2, N'Gia dụng', N'Sản phẩm sử dụng trong gia đình');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Loai] WHERE [Id] = 3)
    INSERT INTO [dbo].[Loai] ([Id], [Ten], [Mota])
    VALUES (3, N'Văn phòng phẩm', N'Dụng cụ học tập và văn phòng');

SET IDENTITY_INSERT [dbo].[Loai] OFF;

SET IDENTITY_INSERT [dbo].[SanPham] ON;

IF NOT EXISTS (SELECT 1 FROM [dbo].[SanPham] WHERE [Id] = 1)
    INSERT INTO [dbo].[SanPham] ([Id], [TenSP], [MoTaSP], [Gia], [SoLuong], [LoaiId], [Ngay])
    VALUES (1, N'Tai nghe Bluetooth', N'Tai nghe không dây mẫu', 590000, 20, 1, '2026-01-01T00:00:00');

IF NOT EXISTS (SELECT 1 FROM [dbo].[SanPham] WHERE [Id] = 2)
    INSERT INTO [dbo].[SanPham] ([Id], [TenSP], [MoTaSP], [Gia], [SoLuong], [LoaiId], [Ngay])
    VALUES (2, N'Ấm siêu tốc', N'Ấm đun nước mẫu', 350000, 15, 2, '2026-01-01T00:00:00');

IF NOT EXISTS (SELECT 1 FROM [dbo].[SanPham] WHERE [Id] = 3)
    INSERT INTO [dbo].[SanPham] ([Id], [TenSP], [MoTaSP], [Gia], [SoLuong], [LoaiId], [Ngay])
    VALUES (3, N'Sổ ghi chép', N'Sổ ghi chép khổ A5', 45000, 100, 3, '2026-01-01T00:00:00');

SET IDENTITY_INSERT [dbo].[SanPham] OFF;
