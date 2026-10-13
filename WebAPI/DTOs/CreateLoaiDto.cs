
using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Loai;

public class CreateLoaiDto
{
    [Required(ErrorMessage = "Tên loại không được để trống")]
    [StringLength(100)]
    public string Ten { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Mota { get; set; }
}
