
namespace QLSanPham.Application.DTOs.SanPham;

public class UpdateSanPhamDto
{
    public string TenSP { get; set; } = string.Empty;

    public string? MoTaSP { get; set; }

    public decimal Gia { get; set; }

    public int SoLuong { get; set; }

    public int LoaiId { get; set; }
}
