namespace ArtGallery.ViewModels
{
    public class ArtistApprovalProcessedViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string AvatarPath { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
    }
} 