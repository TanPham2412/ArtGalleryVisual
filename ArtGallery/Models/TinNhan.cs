using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery.Models
{
    public class TinNhan
    {
        [Key]
        public int MaTinNhan { get; set; }

        [Required]
        public string MaNguoiGui { get; set; }

        [Required]
        public string MaNguoiNhan { get; set; }

        [Required]
        public string NoiDung { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        public bool DaDoc { get; set; } = false;
        public string? DuongDanAnh { get; set; }
        public string? Sticker { get; set; }

        [ForeignKey("MaNguoiGui")]
        public virtual NguoiDung NguoiGui { get; set; }

        [ForeignKey("MaNguoiNhan")]
        public virtual NguoiDung NguoiNhan { get; set; }
    }
}