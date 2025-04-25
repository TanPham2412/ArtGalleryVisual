using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ArtGallery.Models;

public partial class ArtGalleryContext : IdentityDbContext<NguoiDung, IdentityRole, string>
{
    public ArtGalleryContext()
    {
    }

    public ArtGalleryContext(DbContextOptions<ArtGalleryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BinhLuan> BinhLuans { get; set; }

    public virtual DbSet<DoanhThu> DoanhThus { get; set; }

    public virtual DbSet<GiaoDich> GiaoDiches { get; set; }

    public virtual DbSet<LuotThich> LuotThiches { get; set; }

    public virtual DbSet<LuuTranh> LuuTranhs { get; set; }

    public virtual DbSet<Medium> Media { get; set; }

    public virtual DbSet<NoiBat> NoiBats { get; set; }

    public virtual DbSet<TheLoai> TheLoais { get; set; }

    public virtual DbSet<TheTag> TheTags { get; set; }

    public virtual DbSet<TheoDoi> TheoDois { get; set; }

    public virtual DbSet<Tranh> Tranhs { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<PhanHoiBinhLuan> PhanHoiBinhLuans { get; set; }

    public virtual DbSet<TinNhan> TinNhans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Đặt tên bảng theo schema của bạn
        modelBuilder.Entity<NguoiDung>().ToTable("nguoi_dung");
        modelBuilder.Entity<IdentityRole>().ToTable("roles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("user_roles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("user_tokens");

        modelBuilder.Entity<BinhLuan>(entity =>
        {
            entity.HasKey(e => e.MaBinhLuan).HasName("PK__binh_lua__300DD2D8D7F67231");

            entity.ToTable("binh_luan");

            entity.Property(e => e.MaBinhLuan).HasColumnName("ma_binh_luan");
            entity.Property(e => e.MaNguoiDung).HasColumnName("ma_nguoi_dung");
            entity.Property(e => e.MaTranh).HasColumnName("ma_tranh");
            entity.Property(e => e.NgayBinhLuan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_binh_luan");
            entity.Property(e => e.NoiDung).HasColumnName("noi_dung");
            entity.Property(e => e.Rating).HasColumnName("rating").HasDefaultValue(0);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__binh_luan__ma_ng__0E591826");

            entity.HasOne(d => d.MaTranhNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.MaTranh)
                .HasConstraintName("FK__binh_luan__ma_tr__0D64F3ED");
        });

        modelBuilder.Entity<DoanhThu>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__doanh_th__19C32CF7AA9DECC6");

            entity.ToTable("doanh_thu");

            entity.Property(e => e.MaNguoiDung)
                .ValueGeneratedNever()
                .HasColumnName("ma_nguoi_dung");
            entity.Property(e => e.SoTranhBanDuoc)
                .HasDefaultValue(0)
                .HasColumnName("so_tranh_ban_duoc");
            entity.Property(e => e.TongDoanhThu)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("tong_doanh_thu");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithOne(p => p.DoanhThu)
                .HasForeignKey<DoanhThu>(d => d.MaNguoiDung)
                .HasConstraintName("FK__doanh_thu__ma_ng__290D0E62");
        });

        modelBuilder.Entity<GiaoDich>(entity =>
        {
            entity.HasKey(e => e.MaGiaoDich).HasName("PK__giao_dic__FB80ED3254B2912A");

            entity.ToTable("giao_dich");

            entity.Property(e => e.MaGiaoDich).HasColumnName("ma_giao_dich");
            entity.Property(e => e.MaNguoiMua).HasColumnName("ma_nguoi_mua");
            entity.Property(e => e.MaTranh).HasColumnName("ma_tranh");
            entity.Property(e => e.NgayMua)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_mua");
            entity.Property(e => e.SoLuong).HasColumnName("so_luong");
            entity.Property(e => e.SoTien)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("so_tien");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasColumnName("trang_thai");
            entity.Property(e => e.PhuongThucThanhToan)
                .HasMaxLength(100)
                .HasColumnName("phuong_thuc_thanh_toan");
            entity.Property(e => e.IsHiddenByBuyer)
                .HasDefaultValue(false)
                .HasColumnName("is_hidden_by_buyer");
            entity.Property(e => e.IsHiddenBySeller)
                .HasDefaultValue(false)
                .HasColumnName("is_hidden_by_seller");

            entity.HasOne(d => d.MaNguoiMuaNavigation)
                .WithMany(p => p.GiaoDiches)
                .HasForeignKey(d => d.MaNguoiMua)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__giao_dich__ma_ng__2354350C");

            entity.HasOne(d => d.MaTranhNavigation)
                .WithMany(p => p.GiaoDiches)
                .HasForeignKey(d => d.MaTranh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__giao_dich__ma_tr__24485945");
        });

        modelBuilder.Entity<LuotThich>(entity =>
        {
            entity.HasKey(e => e.MaLuotThich).HasName("PK__luot_thi__64AF50517DA222E2");

            entity.ToTable("luot_thich");

            entity.HasIndex(e => new { e.MaTranh, e.MaNguoiDung }, "unique_like").IsUnique();

            entity.Property(e => e.MaLuotThich).HasColumnName("ma_luot_thich");
            entity.Property(e => e.MaNguoiDung).HasColumnName("ma_nguoi_dung");
            entity.Property(e => e.MaTranh).HasColumnName("ma_tranh");
            entity.Property(e => e.NgayThich)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_thich");

            entity.HasOne(d => d.MaNguoiDungNavigation)
                .WithMany(p => p.LuotThiches)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__luot_thic__ma_ng__1411F17C");

            entity.HasOne(d => d.MaTranhNavigation)
                .WithMany(p => p.LuotThiches)
                .HasForeignKey(d => d.MaTranh)
                .HasConstraintName("FK__luot_thic__ma_tr__131DCD43");
        });

        modelBuilder.Entity<LuuTranh>(entity =>
        {
            entity.HasKey(e => e.MaLuuTranh).HasName("PK__luu_tran__0893EFAE9404701B");

            entity.ToTable("luu_tranh");

            entity.HasIndex(e => new { e.MaNguoiDung, e.MaTranh }, "unique_bookmark").IsUnique();

            entity.Property(e => e.MaLuuTranh).HasColumnName("ma_luu_tranh");
            entity.Property(e => e.MaNguoiDung).HasColumnName("ma_nguoi_dung");
            entity.Property(e => e.MaTranh).HasColumnName("ma_tranh");
            entity.Property(e => e.NgayLuu)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_luu");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.LuuTranhs)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__luu_tranh__ma_ng__1D9B5BB6");

            entity.HasOne(d => d.MaTranhNavigation).WithMany(p => p.LuuTranhs)
                .HasForeignKey(d => d.MaTranh)
                .HasConstraintName("FK__luu_tranh__ma_tr__1E8F7FEF");
        });

        modelBuilder.Entity<Medium>(entity =>
        {
            entity.HasKey(e => e.MaMedia).HasName("PK__media__6CED7F1DFD307904");

            entity.ToTable("media");

            entity.Property(e => e.MaMedia).HasColumnName("ma_media");
            entity.Property(e => e.DuongDan).HasColumnName("duong_dan");
            entity.Property(e => e.LoaiMedia)
                .HasMaxLength(50)
                .HasColumnName("loai_media");
            entity.Property(e => e.MaNguoiDung).HasColumnName("ma_nguoi_dung");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.Media)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__media__ma_nguoi___2CDD9F46");
        });

        modelBuilder.Entity<NoiBat>(entity =>
        {
            entity.HasKey(e => new { e.MaNguoiDung, e.MaTranh }).HasName("PK__noi_bat__CFF08069508BECFF");

            entity.ToTable("noi_bat");

            entity.Property(e => e.MaNguoiDung).HasColumnName("ma_nguoi_dung");
            entity.Property(e => e.MaTranh).HasColumnName("ma_tranh");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_tao");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.NoiBats)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__noi_bat__ma_nguo__31A25463");

            entity.HasOne(d => d.MaTranhNavigation).WithMany(p => p.NoiBats)
                .HasForeignKey(d => d.MaTranh)
                .HasConstraintName("FK__noi_bat__ma_tran__3296789C");
        });

        modelBuilder.Entity<TheLoai>(entity =>
        {
            entity.HasKey(e => e.MaTheLoai).HasName("PK__the_loai__489AA0F38C7B2D0E");

            entity.ToTable("the_loai");

            entity.HasIndex(e => e.TenTheLoai, "UQ__the_loai__87296EA3BFC72A9B").IsUnique();

            entity.Property(e => e.MaTheLoai).HasColumnName("ma_the_loai");
            entity.Property(e => e.TenTheLoai)
                .HasMaxLength(50)
                .HasColumnName("ten_the_loai");
        });

        modelBuilder.Entity<TheTag>(entity =>
        {
            entity.HasKey(e => e.MaTag).HasName("PK__the_tag__099A6217E57B8670");

            entity.ToTable("the_tag");

            entity.HasIndex(e => e.TenTag, "UQ__the_tag__56DB79DEC5AA1248").IsUnique();

            entity.Property(e => e.MaTag).HasColumnName("ma_tag");
            entity.Property(e => e.TenTag)
                .HasMaxLength(50)
                .HasColumnName("ten_tag");
        });

        modelBuilder.Entity<TheoDoi>(entity =>
        {
            entity.HasKey(e => new { e.MaNguoiTheoDoi, e.MaNguoiDuocTheoDoi }).HasName("PK__theo_doi__E8A6156EE67C0BC9");

            entity.ToTable("theo_doi");

            entity.Property(e => e.MaNguoiTheoDoi).HasColumnName("ma_nguoi_theo_doi");
            entity.Property(e => e.MaNguoiDuocTheoDoi).HasColumnName("ma_nguoi_duoc_theo_doi");
            entity.Property(e => e.NgayTheoDoi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_theo_doi");

            entity.HasOne(d => d.MaNguoiDuocTheoDoiNavigation).WithMany(p => p.TheoDoiMaNguoiDuocTheoDoiNavigations)
                .HasForeignKey(d => d.MaNguoiDuocTheoDoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__theo_doi__ma_ngu__18D6A699");

            entity.HasOne(d => d.MaNguoiTheoDoiNavigation).WithMany(p => p.TheoDoiMaNguoiTheoDoiNavigations)
                .HasForeignKey(d => d.MaNguoiTheoDoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__theo_doi__ma_ngu__17E28260");
        });

        modelBuilder.Entity<Tranh>(entity =>
        {
            entity.HasKey(e => e.MaTranh).HasName("PK__tranh__633AC9ECC367B622");

            entity.ToTable("tranh");

            entity.Property(e => e.MaTranh).HasColumnName("ma_tranh");
            entity.Property(e => e.DaBan).HasColumnName("da_ban");
            entity.Property(e => e.DuongDanAnh).HasColumnName("duong_dan_anh");
            entity.Property(e => e.Gia)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("gia");
            entity.Property(e => e.MaNguoiDung).HasColumnName("ma_nguoi_dung");
            entity.Property(e => e.MoTa).HasColumnName("mo_ta");
            entity.Property(e => e.NgayDang)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_dang");
            entity.Property(e => e.SoLuongTon).HasColumnName("so_luong_ton");
            entity.Property(e => e.TieuDe)
                .HasMaxLength(255)
                .HasColumnName("tieu_de");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Đang bán")
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.Tranhs)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__tranh__ma_nguoi___7C3A67EB");

            entity.HasMany(d => d.MaTags).WithMany(p => p.MaTranhs)
                .UsingEntity<Dictionary<string, object>>(
                    "TranhTheTag",
                    r => r.HasOne<TheTag>().WithMany()
                        .HasForeignKey("MaTag")
                        .HasConstraintName("FK__tranh_the__ma_ta__09946309"),
                    l => l.HasOne<Tranh>().WithMany()
                        .HasForeignKey("MaTranh")
                        .HasConstraintName("FK__tranh_the__ma_tr__08A03ED0"),
                    j =>
                    {
                        j.HasKey("MaTranh", "MaTag").HasName("PK__tranh_th__03A36FCDE94B1BA4");
                        j.ToTable("tranh_the_tag");
                        j.IndexerProperty<int>("MaTranh").HasColumnName("ma_tranh");
                        j.IndexerProperty<int>("MaTag").HasColumnName("ma_tag");
                    });

            entity.HasMany(d => d.MaTheLoais).WithMany(p => p.MaTranhs)
                .UsingEntity<Dictionary<string, object>>(
                    "TranhTheLoai",
                    r => r.HasOne<TheLoai>().WithMany()
                        .HasForeignKey("MaTheLoai")
                        .HasConstraintName("FK__tranh_the__ma_th__02E7657A"),
                    l => l.HasOne<Tranh>().WithMany()
                        .HasForeignKey("MaTranh")
                        .HasConstraintName("FK__tranh_the__ma_tr__01F34141"),
                    j =>
                    {
                        j.HasKey("MaTranh", "MaTheLoai").HasName("PK__tranh_th__47B363E3D8A01C76");
                        j.ToTable("tranh_the_loai");
                        j.IndexerProperty<int>("MaTranh").HasColumnName("ma_tranh");
                        j.IndexerProperty<int>("MaTheLoai").HasColumnName("ma_the_loai");
                    });
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.MaThongBao);

            entity.ToTable("ThongBao");

            entity.Property(e => e.MaThongBao).HasColumnName("ma_thong_bao");

            entity.Property(e => e.MaNguoiNhan)
                .HasMaxLength(450)
                .HasColumnName("ma_nguoi_nhan");

            entity.Property(e => e.MaNguoiGui)
                .HasMaxLength(450)
                .HasColumnName("ma_nguoi_gui");

            entity.Property(e => e.TieuDe)
                .HasMaxLength(200)
                .HasColumnName("tieu_de");

            entity.Property(e => e.NoiDung)
                .HasColumnName("noi_dung");

            entity.Property(e => e.DuongDanAnh)
                .HasMaxLength(500)
                .HasColumnName("duong_dan_anh");

            entity.Property(e => e.URL)
                .HasMaxLength(500)
                .HasColumnName("url");

            entity.Property(e => e.LoaiThongBao)
                .HasMaxLength(50)
                .HasColumnName("loai_thong_bao");

            entity.Property(e => e.DaDoc)
                .HasColumnName("da_doc")
                .HasDefaultValueSql("((0))");

            entity.Property(e => e.ThoiGian)
                .HasColumnType("datetime")
                .HasColumnName("thoi_gian");

            entity.HasOne(d => d.MaNguoiNhanNavigation)
                .WithMany()
                .HasForeignKey(d => d.MaNguoiNhan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThongBao_AspNetUsers_NguoiNhan");

            entity.HasOne(d => d.MaNguoiGuiNavigation)
                .WithMany()
                .HasForeignKey(d => d.MaNguoiGui)
                .HasConstraintName("FK_ThongBao_AspNetUsers_NguoiGui");
        });

        modelBuilder.Entity<PhanHoiBinhLuan>(entity =>
        {
            entity.HasKey(e => e.MaPhanHoi).HasName("PK__phan_hoi_binh_luan__ID");
            
            entity.ToTable("phan_hoi_binh_luan");
            
            entity.Property(e => e.MaPhanHoi).HasColumnName("ma_phan_hoi");
            entity.Property(e => e.MaBinhLuan).HasColumnName("ma_binh_luan");
            entity.Property(e => e.MaNguoiDung).HasColumnName("ma_nguoi_dung");
            entity.Property(e => e.NoiDung).HasColumnName("noi_dung");
            entity.Property(e => e.NgayPhanHoi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_phan_hoi");
            
            entity.HasOne(d => d.MaBinhLuanNavigation)
                .WithMany()
                .HasForeignKey(d => d.MaBinhLuan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__phan_hoi__ma_binh_luan");
            
            entity.HasOne(d => d.MaNguoiDungNavigation)
                .WithMany()
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__phan_hoi__ma_nguoi_dung");
        });

        modelBuilder.Entity<TinNhan>(entity =>
        {
            entity.HasKey(e => e.MaTinNhan).HasName("PK__tin_nhan__ID");

            entity.ToTable("tin_nhan");

            entity.Property(e => e.MaTinNhan).HasColumnName("ma_tin_nhan");
            entity.Property(e => e.MaNguoiGui).HasColumnName("ma_nguoi_gui");
            entity.Property(e => e.MaNguoiNhan).HasColumnName("ma_nguoi_nhan");
            entity.Property(e => e.NoiDung).HasColumnName("noi_dung");
            entity.Property(e => e.ThoiGian)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("thoi_gian");
            entity.Property(e => e.DaDoc)
                .HasDefaultValue(false)
                .HasColumnName("da_doc");

            entity.HasOne(d => d.NguoiGui)
                .WithMany()
                .HasForeignKey(d => d.MaNguoiGui)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tin_nhan__ma_nguoi_gui");

            entity.HasOne(d => d.NguoiNhan)
                .WithMany()
                .HasForeignKey(d => d.MaNguoiNhan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tin_nhan__ma_nguoi_nhan");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    // Thêm phương thức OnConfiguring để cấu hình connection string cho EF Core tools
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Cấu hình connection string cho dotnet ef migrations
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("ART_GALLERY", EnvironmentVariableTarget.User));
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
