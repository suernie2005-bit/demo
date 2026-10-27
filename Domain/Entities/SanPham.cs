using System;
using System.Collections.Generic;

namespace QLSanPham.Domain.Entities;

public partial class SanPham
{
    public int Id { get; set; }

    public string TenSp { get; set; } = null!;

    public string? MoTaSp { get; set; }

    public decimal Gia { get; set; }

    public int SoLuong { get; set; }

    public int LoaiId { get; set; }

    public DateTime Ngay { get; set; }

    public virtual Loai Loai { get; set; } = null!;
}
