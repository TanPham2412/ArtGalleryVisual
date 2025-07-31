using System;

namespace ArtGallery.Models;

public partial class LichSuChinhSuaPhanHoi
{
    public int MaLichSu { get; set; }

    public int MaPhanHoi { get; set; }

    public string NoiDungCu { get; set; } = null!;

    public string NoiDungMoi { get; set; } = null!;

    public DateTime NgayChinhSua { get; set; }
    
    public string? DuongDanAnhCu { get; set; }
    
    public string? DuongDanAnhMoi { get; set; }
    
    public string? StickerCu { get; set; }
    
    public string? StickerMoi { get; set; }

    public virtual PhanHoiBinhLuan MaPhanHoiNavigation { get; set; } = null!;
}