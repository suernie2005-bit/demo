using System;
using System.Collections.Generic;

namespace WebAPI.Models;

public partial class Loai
{
    public int Id { get; set; }

    public string Ten { get; set; } = null!;

    public string? Mota { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
