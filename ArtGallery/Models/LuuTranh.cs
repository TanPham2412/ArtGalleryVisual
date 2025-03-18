using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class LuuTranh
{
    public int MaLuuTranh { get; set; }

    public int MaNguoiDung { get; set; }

    public int MaTranh { get; set; }

    public DateTime? NgayLuu { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    public virtual Tranh MaTranhNavigation { get; set; } = null!;
}
