using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using QLSanPham.Domain.Entities;
using QLSanPham.Infrastructure.Identity;

namespace QLSanPham.Infrastructure.Persistence;

public partial class QLSanPhamDBContext : IdentityDbContext<ApplicationUser>
{
    public QLSanPhamDBContext(
        DbContextOptions<QLSanPhamDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Loai> Loais { get; set; }
    public virtual DbSet<SanPham> SanPhams { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(user => user.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("EmailIndex")
                .HasFilter("[NormalizedEmail] IS NOT NULL");
        });

        modelBuilder.Entity<Loai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Loai");

            entity.ToTable("Loai");

            entity.Property(e => e.Ten)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.Ten)
                .IsUnique()
                .HasDatabaseName("UX_Loai_Ten");

            entity.Property(e => e.Mota)
                .HasMaxLength(255);

            entity.HasData(
                new Loai { Id = 1, Ten = "Điện tử", Mota = "Thiết bị và phụ kiện điện tử" },
                new Loai { Id = 2, Ten = "Gia dụng", Mota = "Sản phẩm sử dụng trong gia đình" },
                new Loai { Id = 3, Ten = "Văn phòng phẩm", Mota = "Dụng cụ học tập và văn phòng" });
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SanPham");

            entity.ToTable("SanPham", table =>
            {
                table.HasCheckConstraint("CK_SanPham_Gia_Positive", "[Gia] > 0");
                table.HasCheckConstraint("CK_SanPham_SoLuong_NonNegative", "[SoLuong] >= 0");
            });

            entity.Property(e => e.TenSp)
                .HasColumnName("TenSP")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.MoTaSp)
                .HasColumnName("MoTaSP")
                .HasMaxLength(500);

            entity.Property(e => e.Gia)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Ngay)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(e => new { e.LoaiId, e.TenSp })
                .IsUnique()
                .HasDatabaseName("UX_SanPham_LoaiId_TenSP");

            entity.HasOne(d => d.Loai)
                .WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.LoaiId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_SanPham_Loai");

            entity.HasData(
                new SanPham
                {
                    Id = 1,
                    TenSp = "Tai nghe Bluetooth",
                    MoTaSp = "Tai nghe không dây mẫu",
                    Gia = 590000m,
                    SoLuong = 20,
                    LoaiId = 1,
                    Ngay = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new SanPham
                {
                    Id = 2,
                    TenSp = "Ấm siêu tốc",
                    MoTaSp = "Ấm đun nước mẫu",
                    Gia = 350000m,
                    SoLuong = 15,
                    LoaiId = 2,
                    Ngay = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new SanPham
                {
                    Id = 3,
                    TenSp = "Sổ ghi chép",
                    MoTaSp = "Sổ ghi chép khổ A5",
                    Gia = 45000m,
                    SoLuong = 100,
                    LoaiId = 3,
                    Ngay = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshToken", table =>
            {
                table.HasCheckConstraint(
                    "CK_RefreshToken_ExpiresAfterCreated",
                    "[ExpiresAtUtc] > [CreatedAtUtc]");
                table.HasCheckConstraint(
                    "CK_RefreshToken_TokenHash_Hex64",
                    "LEN([TokenHash]) = 64 AND [TokenHash] NOT LIKE '%[^0-9a-fA-F]%'");
                table.HasCheckConstraint(
                    "CK_RefreshToken_RevokedAfterCreated",
                    "[RevokedAtUtc] IS NULL OR [RevokedAtUtc] >= [CreatedAtUtc]");
            });

            entity.HasKey(refreshToken => refreshToken.Id)
                .HasName("PK_RefreshToken");

            entity.Property(refreshToken => refreshToken.UserId)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(refreshToken => refreshToken.TokenHash)
                .HasColumnType("char(64)")
                .IsRequired();

            entity.Property(refreshToken => refreshToken.FamilyId)
                .IsRequired();

            entity.Property(refreshToken => refreshToken.CreatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            entity.Property(refreshToken => refreshToken.ExpiresAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            entity.Property(refreshToken => refreshToken.RevokedAtUtc)
                .HasColumnType("datetime2");

            entity.HasIndex(refreshToken => refreshToken.TokenHash)
                .IsUnique()
                .HasDatabaseName("UX_RefreshToken_TokenHash");

            entity.HasIndex(refreshToken => new
                {
                    refreshToken.UserId,
                    refreshToken.FamilyId
                })
                .HasDatabaseName("IX_RefreshToken_UserId_FamilyId");

            entity.HasIndex(refreshToken => new
                {
                    refreshToken.FamilyId,
                    refreshToken.RevokedAtUtc,
                    refreshToken.ExpiresAtUtc
                })
                .HasDatabaseName("IX_RefreshToken_FamilyId_RevokedAtUtc_ExpiresAtUtc");

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RefreshToken_AspNetUsers_UserId");

            entity.HasOne(refreshToken => refreshToken.ReplacedByToken)
                .WithOne()
                .HasForeignKey<RefreshToken>(refreshToken => refreshToken.ReplacedByTokenId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RefreshToken_ReplacedByToken");
        });
    }
}
