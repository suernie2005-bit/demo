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
            entity.HasKey(e => e.Id);

            entity.ToTable("Loai");

            entity.Property(e => e.Ten).HasMaxLength(100).IsRequired();

            entity.HasIndex(e => e.Ten).IsUnique();

            entity.Property(e => e.Mota).HasMaxLength(255);
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("SanPham");

            entity.Property(e => e.TenSp).HasMaxLength(150).IsRequired();

            entity.Property(e => e.MoTaSp).HasMaxLength(500);

            entity.Property(e => e.Gia).HasColumnType("decimal(18,2)");

            entity.Property(e => e.Ngay).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(d => d.Loai).WithMany(p => p.SanPhams).HasForeignKey(d => d.LoaiId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}