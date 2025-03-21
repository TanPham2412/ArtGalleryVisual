using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class LuotThich
{
    public int MaLuotThich { get; set; }

    public int MaTranh { get; set; }

    public string MaNguoiDung { get; set; } = null!;

    public DateTime? NgayThich { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    public virtual Tranh MaTranhNavigation { get; set; } = null!;
}
