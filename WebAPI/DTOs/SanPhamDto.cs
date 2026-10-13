
namespace WebAPI.DTOs.SanPham;

public class SanPhamDto
{
    public int Id { get; set; }
    public string TenSP { get; set; } = string.Empty;
    public string? MoTaSP { get; set; }
    public decimal Gia { get; set; }
    public int SoLuong { get; set; }
    public int LoaiId { get; set; }
    public string TenLoai { get; set; } = string.Empty;
    public DateTime Ngay { get; set; }
}
