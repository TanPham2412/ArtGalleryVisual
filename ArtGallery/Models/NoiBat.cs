using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class NoiBat
{
    public int MaNguoiDung { get; set; }

    public int MaTranh { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    public virtual Tranh MaTranhNavigation { get; set; } = null!;
}
