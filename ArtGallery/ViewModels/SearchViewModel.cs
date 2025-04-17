using ArtGallery.Models;

namespace ArtGallery.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; }
        public string Tag { get; set; }
        public string Category { get; set; } = "Top";
        public string SortBy { get; set; } = "newest";
        public IEnumerable<Tranh> Artworks { get; set; } = new List<Tranh>();
        public IEnumerable<NguoiDung> Artists { get; set; } = new List<NguoiDung>();
        public int TotalResults => Category == "Artists" ? Artists.Count() : Artworks.Count();
        public IEnumerable<TheLoai> Categories { get; set; } = new List<TheLoai>();
        public bool IsTagSearch { get; set; }
        public List<TheTag> Tags { get; set; } = new List<TheTag>();

        private readonly string _currentUserId;
        private readonly IEnumerable<TheoDoi> _follows;

        public SearchViewModel(string currentUserId = null, IEnumerable<TheoDoi> follows = null)
        {
            _currentUserId = currentUserId;
            _follows = follows ?? new List<TheoDoi>();
        }

        public bool IsFollowing(string artistId)
        {
            if (string.IsNullOrEmpty(_currentUserId)) return false;
            return _follows.Any(f => f.MaNguoiTheoDoi == _currentUserId && f.MaNguoiDuocTheoDoi == artistId);
        }
    }
}