using Microsoft.AspNetCore.Mvc.Rendering;
using ArtGallery.Models;

namespace ArtGallery.Repositories.Interfaces
{
    public interface IArtworkRepository
    {
        Task<(Tranh artwork, List<Tranh> otherWorks)> GetArtworkForDisplay(int id);
        Task<(TheLoai category, List<Tranh> artworks)> GetArtworksByCategory(int categoryId);
        Task<(TheTag tag, List<Tranh> artworks)> GetArtworksByTag(int tagId);
        Task<Tranh> GetArtworkForEdit(int id, string currentUserId, bool isAdmin = false);
        Task<List<SelectListItem>> GetCategories();
        Task<(bool success, string message)> UpdateArtwork(Tranh model, IFormFile imageFile, string tagsInput, List<int> selectedCategories, string currentUserId, bool isAdmin = false);
        Task<(bool success, string message, string redirectUrl)> DeleteArtwork(int id, string currentUserId);
        Task<List<Tranh>> GetAllArtworks();
        Task<List<Tranh>> GetFilteredArtworks(string searchString, string sortOrder);
        Task<(bool success, bool liked, string message)> ToggleLike(int artworkId, string userId);
    }
}
