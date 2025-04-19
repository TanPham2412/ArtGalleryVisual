using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class BinhLuan
{
    public int MaBinhLuan { get; set; }

    public int MaTranh { get; set; }

    public string MaNguoiDung { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public DateTime? NgayBinhLuan { get; set; }
    
    public int Rating { get; set; } = 0;

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    public virtual Tranh MaTranhNavigation { get; set; } = null!;
}
