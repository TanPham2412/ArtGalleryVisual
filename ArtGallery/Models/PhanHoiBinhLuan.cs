using System;

namespace ArtGallery.Models;

public partial class PhanHoiBinhLuan
{
    public int MaPhanHoi { get; set; }

    public int MaBinhLuan { get; set; } // Bình luận gốc

    public string MaNguoiDung { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public DateTime? NgayPhanHoi { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    public virtual BinhLuan MaBinhLuanNavigation { get; set; } = null!;
} 