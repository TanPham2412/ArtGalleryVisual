using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models;

public partial class NguoiDung : IdentityUser<string>
{
    private string _userName;
    private string _tenNguoiDung;
    
    public NguoiDung()
    {
        Id = Guid.NewGuid().ToString();
        NgayTao = DateTime.Now;
    }

    // Ghi đè thuộc tính UserName để tự động cập nhật TenNguoiDung
    public override string UserName
    {
        get => _userName;
        set
        {
            _userName = value!;
            // Tự động thiết lập TenNguoiDung khi UserName được gán giá trị
            if (string.IsNullOrEmpty(TenNguoiDung) && !string.IsNullOrEmpty(value))
            {
                TenNguoiDung = value;
            }
        }
    }

    [Required]
    public string TenNguoiDung 
    { 
        get => _tenNguoiDung ?? UserName ?? Email?.Split('@')[0] ?? "User_" + Id?.Substring(0, 8);
        set => _tenNguoiDung = value!;
    }

    public string? DiaChi { get; set; }

    public string? GioiTinh { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? BaiHat { get; set; }

    public string? AnhDaiDien { get; set; }

    public string? CoverImage { get; set; }

    public string? MoTa { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? HienThiGioiTinh { get; set; }

    public string? HienThiDiaChi { get; set; }

    public string? HienThiNgaySinh { get; set; }

    public string? HienThiNamSinh { get; set; }

    public bool DangKyNgheSi { get; set; } = false;

    public DateTime NgayDangKy { get; set; } = DateTime.Now;

    public virtual DoanhThu? DoanhThu { get; set; }

    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

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
        return $"/images/authors/avatars/{UserName}/{AnhDaiDien}";
    }
}
