using System;

namespace ArtGallery.Models;

public partial class LichSuChinhSuaBinhLuan
{
    public int MaLichSu { get; set; }

    public int MaBinhLuan { get; set; }

    public string NoiDungCu { get; set; } = null!;

    public string NoiDungMoi { get; set; } = null!;

    public DateTime NgayChinhSua { get; set; }
    
    public string? DuongDanAnhCu { get; set; }
    
    public string? DuongDanAnhMoi { get; set; }
    
    public string? StickerCu { get; set; }
    
    public string? StickerMoi { get; set; }
    
    public int RatingCu { get; set; }
    
    public int RatingMoi { get; set; }

    public virtual BinhLuan MaBinhLuanNavigation { get; set; } = null!;
}