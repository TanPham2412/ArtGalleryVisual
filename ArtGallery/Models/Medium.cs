using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class Medium
{
    public int MaMedia { get; set; }

    public string MaNguoiDung { get; set; }

    public string? LoaiMedia { get; set; }

    public string DuongDan { get; set; } = null!;

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
