-- Generated from EF Core migrations through 20261027121839_AddIdentityAndRefreshTokenPersistence.
-- Clean-database bootstrap script only. It creates catalog, Identity and refresh-token schema,
-- and inserts the catalog seed data. Do not run against a database that must be preserved.
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

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [RefreshToken] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [TokenHash] char(64) NOT NULL,
    [FamilyId] uniqueidentifier NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [ExpiresAtUtc] datetime2 NOT NULL,
    [RevokedAtUtc] datetime2 NULL,
    [ReplacedByTokenId] uniqueidentifier NULL,
    CONSTRAINT [PK_RefreshToken] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_RefreshToken_ExpiresAfterCreated] CHECK ([ExpiresAtUtc] > [CreatedAtUtc]),
    CONSTRAINT [CK_RefreshToken_TokenHash_Hex64] CHECK (LEN([TokenHash]) = 64 AND [TokenHash] NOT LIKE '%[^0-9a-fA-F]%'),
    CONSTRAINT [CK_RefreshToken_RevokedAfterCreated] CHECK ([RevokedAtUtc] IS NULL OR [RevokedAtUtc] >= [CreatedAtUtc]),
    CONSTRAINT [FK_RefreshToken_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RefreshToken_ReplacedByToken] FOREIGN KEY ([ReplacedByTokenId]) REFERENCES [RefreshToken] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE UNIQUE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]) WHERE [NormalizedEmail] IS NOT NULL;
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_RefreshToken_FamilyId_RevokedAtUtc_ExpiresAtUtc] ON [RefreshToken] ([FamilyId], [RevokedAtUtc], [ExpiresAtUtc]);
GO

CREATE UNIQUE INDEX [IX_RefreshToken_ReplacedByTokenId] ON [RefreshToken] ([ReplacedByTokenId]) WHERE [ReplacedByTokenId] IS NOT NULL;
GO

CREATE INDEX [IX_RefreshToken_UserId_FamilyId] ON [RefreshToken] ([UserId], [FamilyId]);
GO

CREATE UNIQUE INDEX [UX_RefreshToken_TokenHash] ON [RefreshToken] ([TokenHash]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261027121839_AddIdentityAndRefreshTokenPersistence', N'8.0.0');
GO

COMMIT;
GO

