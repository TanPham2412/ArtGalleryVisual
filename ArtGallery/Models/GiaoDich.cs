﻿using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class GiaoDich
{
    public int MaGiaoDich { get; set; }

    public string MaNguoiMua { get; set; } = null!;

    public int MaTranh { get; set; }

    public int SoLuong { get; set; }

    public decimal SoTien { get; set; }

    public string TrangThai { get; set; }

    public string PhuongThucThanhToan { get; set; } = null!;

    public DateTime? NgayMua { get; set; }

    public bool? IsHiddenByBuyer { get; set; }

    public bool? IsHiddenBySeller { get; set; }

    public virtual NguoiDung MaNguoiMuaNavigation { get; set; } = null!;

    public virtual Tranh MaTranhNavigation { get; set; } = null!;
}
