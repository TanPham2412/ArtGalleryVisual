using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class NguoiDung
{
    public int MaNguoiDung { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string TenNguoiDung { get; set; } = null!;

    public string? DiaChi { get; set; }

    public string? GioiTinh { get; set; }

    public string? SoDienThoai { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? LoaiNguoiDung { get; set; }

    public string? BaiHat { get; set; }

    public string? AnhDaiDien { get; set; }

    public string? CoverImage { get; set; }

    public string? MoTa { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? HienThiGioiTinh { get; set; }

    public string? HienThiDiaChi { get; set; }

    public string? HienThiNgaySinh { get; set; }

    public string? HienThiNamSinh { get; set; }

    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

    public virtual DoanhThu? DoanhThu { get; set; }

    public virtual ICollection<GiaoDich> GiaoDiches { get; set; } = new List<GiaoDich>();

    public virtual ICollection<LuotThich> LuotThiches { get; set; } = new List<LuotThich>();

    public virtual ICollection<LuuTranh> LuuTranhs { get; set; } = new List<LuuTranh>();

    public virtual ICollection<Medium> Media { get; set; } = new List<Medium>();

    public virtual ICollection<NoiBat> NoiBats { get; set; } = new List<NoiBat>();

    public virtual ICollection<TheoDoi> TheoDoiMaNguoiDuocTheoDoiNavigations { get; set; } = new List<TheoDoi>();

    public virtual ICollection<TheoDoi> TheoDoiMaNguoiTheoDoiNavigations { get; set; } = new List<TheoDoi>();

    public virtual ICollection<Tranh> Tranhs { get; set; } = new List<Tranh>();

    public string GetAvatarPath()
    {
        if (string.IsNullOrEmpty(AnhDaiDien))
        {
            return "/images/authors/default/default-image.png";
        }
        return $"/images/authors/avatars/{TenDangNhap}/{AnhDaiDien}";
    }
}
