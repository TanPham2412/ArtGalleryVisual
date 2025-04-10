using ArtGallery.Models;

namespace ArtGallery.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; }
        public string Category { get; set; } = "Top";
        public string SortBy { get; set; } = "newest";
        public IEnumerable<Tranh> Artworks { get; set; } = new List<Tranh>();
        public IEnumerable<NguoiDung> Artists { get; set; } = new List<NguoiDung>();
        public int TotalResults => Category == "Artists" ? Artists.Count() : Artworks.Count();
        public IEnumerable<TheLoai> Categories { get; set; } = new List<TheLoai>();
    }
}