
using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.SanPham;

public class UpdateSanPhamDto
{
    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [StringLength(150)]
    public string TenSP { get; set; } = string.Empty;

    [StringLength(500)]
    public string? MoTaSP { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999",
        ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal Gia { get; set; }

    [Range(0, int.MaxValue,
        ErrorMessage = "Số lượng không được âm")]
    public int SoLuong { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Vui lòng chọn loại sản phẩm")]
    public int LoaiId { get; set; }
}
