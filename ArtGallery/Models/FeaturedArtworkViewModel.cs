using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models
{
    public class FeaturedArtworkViewModel
    {
        [Required]
        public int ArtworkId { get; set; }
        
        public bool IsFeatured { get; set; }
    }
} 