using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models;

public partial class QlsanphamDbContext : DbContext
{
    public QlsanphamDbContext()
    {
    }

    public QlsanphamDbContext(DbContextOptions<QlsanphamDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Loai> Loais { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-SU6V9B8E.;Database=QLSanphamDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Loai__3214EC0709122FBB");

            entity.ToTable("Loai");

            entity.HasIndex(e => e.Ten, "UQ__Loai__C451FA835BF9129C").IsUnique();

            entity.Property(e => e.Mota).HasMaxLength(255);
            entity.Property(e => e.Ten).HasMaxLength(100);
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SanPham__3214EC0764E2950D");

            entity.ToTable("SanPham");

            entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MoTaSp)
                .HasMaxLength(500)
                .HasColumnName("MoTaSP");
            entity.Property(e => e.Ngay).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.TenSp)
                .HasMaxLength(150)
                .HasColumnName("TenSP");

            entity.HasOne(d => d.Loai).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.LoaiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SanPham_Loa");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
