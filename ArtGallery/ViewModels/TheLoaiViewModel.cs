using ArtGallery.Models;
using System.Collections.Generic;

namespace ArtGallery.ViewModels
{
    public class TheLoaiViewModel
    {
        public TheLoai TheLoai { get; set; }
        public List<Tranh> Artworks { get; set; }
        public string CategoryName => TheLoai?.TenTheLoai;
    }
} 