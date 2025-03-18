using Microsoft.AspNetCore.Mvc.Rendering;
using ArtGallery.Models;

namespace ArtGallery.Repositories.Interfaces
{
    public interface IArtworkRepository
    {
        Task<(Tranh artwork, List<Tranh> otherWorks)> GetArtworkForDisplay(int id);
        Task<(TheLoai category, List<Tranh> artworks)> GetArtworksByCategory(int categoryId);
        Task<(TheTag tag, List<Tranh> artworks)> GetArtworksByTag(int tagId);
        Task<Tranh> GetArtworkForEdit(int id, int currentUserId);
        Task<List<SelectListItem>> GetCategories();
        Task<(bool success, string message)> UpdateArtwork(Tranh model, IFormFile imageFile, string tagsInput, List<int> selectedCategories, int currentUserId);
        Task<(bool success, string message, string redirectUrl)> DeleteArtwork(int id, int currentUserId);
    }
}
