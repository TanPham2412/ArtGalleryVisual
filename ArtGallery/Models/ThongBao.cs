using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class ThongBao
{
    public int MaThongBao { get; set; }

    public string MaNguoiNhan { get; set; } = null!;

    public string? MaNguoiGui { get; set; }

    public string TieuDe { get; set; } = null!;

    public string NoiDung { get; set; } = null!;
    
    public string? DuongDanAnh { get; set; }
    
    public string? URL { get; set; }
    
    public string? LoaiThongBao { get; set; }
    
    public bool? DaDoc { get; set; }
    
    public DateTime ThoiGian { get; set; }

    public virtual NguoiDung MaNguoiNhanNavigation { get; set; } = null!;
    
    public virtual NguoiDung? MaNguoiGuiNavigation { get; set; }
} 