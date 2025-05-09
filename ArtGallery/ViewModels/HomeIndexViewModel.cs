using ArtGallery.Models;

namespace ArtGallery.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Tranh> FollowingArtworks { get; set; } = new List<Tranh>();
        public List<Tranh> MostLikedArtworks { get; set; } = new List<Tranh>();
        public List<Tranh> LatestArtworks { get; set; } = new List<Tranh>();
        public string ActiveCategory { get; set; } = "Home";
    }
} 