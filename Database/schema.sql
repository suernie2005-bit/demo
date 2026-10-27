IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Loai] (
    [Id] int NOT NULL IDENTITY,
    [Ten] nvarchar(100) NOT NULL,
    [Mota] nvarchar(255) NULL,
    CONSTRAINT [PK_Loai] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [SanPham] (
    [Id] int NOT NULL IDENTITY,
    [TenSP] nvarchar(150) NOT NULL,
    [MoTaSP] nvarchar(500) NULL,
    [Gia] decimal(18,2) NOT NULL,
    [SoLuong] int NOT NULL,
    [LoaiId] int NOT NULL,
    [Ngay] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_SanPham] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_SanPham_Gia_Positive] CHECK ([Gia] > 0),
    CONSTRAINT [CK_SanPham_SoLuong_NonNegative] CHECK ([SoLuong] >= 0),
    CONSTRAINT [FK_SanPham_Loai] FOREIGN KEY ([LoaiId]) REFERENCES [Loai] ([Id]) ON DELETE NO ACTION
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Mota', N'Ten') AND [object_id] = OBJECT_ID(N'[Loai]'))
    SET IDENTITY_INSERT [Loai] ON;
INSERT INTO [Loai] ([Id], [Mota], [Ten])
VALUES (1, N'Thiết bị và phụ kiện điện tử', N'Điện tử'),
(2, N'Sản phẩm sử dụng trong gia đình', N'Gia dụng'),
(3, N'Dụng cụ học tập và văn phòng', N'Văn phòng phẩm');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Mota', N'Ten') AND [object_id] = OBJECT_ID(N'[Loai]'))
    SET IDENTITY_INSERT [Loai] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Gia', N'LoaiId', N'MoTaSP', N'Ngay', N'SoLuong', N'TenSP') AND [object_id] = OBJECT_ID(N'[SanPham]'))
    SET IDENTITY_INSERT [SanPham] ON;
INSERT INTO [SanPham] ([Id], [Gia], [LoaiId], [MoTaSP], [Ngay], [SoLuong], [TenSP])
VALUES (1, 590000.0, 1, N'Tai nghe không dây mẫu', '2026-01-01T00:00:00.0000000Z', 20, N'Tai nghe Bluetooth'),
(2, 350000.0, 2, N'Ấm đun nước mẫu', '2026-01-01T00:00:00.0000000Z', 15, N'Ấm siêu tốc'),
(3, 45000.0, 3, N'Sổ ghi chép khổ A5', '2026-01-01T00:00:00.0000000Z', 100, N'Sổ ghi chép');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Gia', N'LoaiId', N'MoTaSP', N'Ngay', N'SoLuong', N'TenSP') AND [object_id] = OBJECT_ID(N'[SanPham]'))
    SET IDENTITY_INSERT [SanPham] OFF;
GO

CREATE UNIQUE INDEX [UX_Loai_Ten] ON [Loai] ([Ten]);
GO

CREATE UNIQUE INDEX [UX_SanPham_LoaiId_TenSP] ON [SanPham] ([LoaiId], [TenSP]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261027072150_InitialCreate', N'8.0.0');
GO

COMMIT;
GO

