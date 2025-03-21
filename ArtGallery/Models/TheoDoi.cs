using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class TheoDoi
{
    public string MaNguoiTheoDoi { get; set; }

    public string MaNguoiDuocTheoDoi { get; set; }

    public DateTime? NgayTheoDoi { get; set; }

    public virtual NguoiDung MaNguoiDuocTheoDoiNavigation { get; set; } = null!;

    public virtual NguoiDung MaNguoiTheoDoiNavigation { get; set; } = null!;
}
