using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class BinhLuan
{
    public int MaBinhLuan { get; set; }

    public int MaTranh { get; set; }

    public int MaNguoiDung { get; set; }

    public string NoiDung { get; set; } = null!;

    public DateTime? NgayBinhLuan { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    public virtual Tranh MaTranhNavigation { get; set; } = null!;
}
