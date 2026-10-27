using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Data;

public partial class QLSanPhamDBContext : DbContext
{
    public QLSanPhamDBContext(
        DbContextOptions<QLSanPhamDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Loai> Loais { get; set; }
    public virtual DbSet<SanPham> SanPhams { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    }
}
