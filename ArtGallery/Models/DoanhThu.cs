using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class DoanhThu
{
    public int MaNguoiDung { get; set; }

    public decimal? TongDoanhThu { get; set; }

    public int? SoTranhBanDuoc { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
