using ArtGallery.Models;

namespace ArtGallery.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; }
        public string Category { get; set; } = "Top";
        public string SortBy { get; set; } = "newest";
        public IEnumerable<Tranh> Artworks { get; set; } = new List<Tranh>();
        public int TotalResults => Artworks.Count();
        public IEnumerable<TheLoai> Categories { get; set; } = new List<TheLoai>();
    }
} 