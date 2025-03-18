using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class Tranh
{
    public int MaTranh { get; set; }

    public int MaNguoiDung { get; set; }

    public string TieuDe { get; set; } = null!;

    public string? MoTa { get; set; }

    public string DuongDanAnh { get; set; } = null!;

    public DateTime? NgayDang { get; set; }

    public decimal Gia { get; set; }

    public string? TrangThai { get; set; }

    public int SoLuongTon { get; set; }

    public int DaBan { get; set; }

    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

    public virtual ICollection<GiaoDich> GiaoDiches { get; set; } = new List<GiaoDich>();

    public virtual ICollection<LuotThich> LuotThiches { get; set; } = new List<LuotThich>();

    public virtual ICollection<LuuTranh> LuuTranhs { get; set; } = new List<LuuTranh>();

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    public virtual ICollection<NoiBat> NoiBats { get; set; } = new List<NoiBat>();

    public virtual ICollection<TheTag> MaTags { get; set; } = new List<TheTag>();

    public virtual ICollection<TheLoai> MaTheLoais { get; set; } = new List<TheLoai>();
}
