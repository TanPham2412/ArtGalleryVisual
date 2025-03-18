using Microsoft.AspNetCore.Mvc.Rendering;
using ArtGallery.Models;

namespace ArtGallery.Repositories.Interfaces
{
    public interface IHomeRepository
    {
        Task<List<Tranh>> GetRandomArtworksFromFollowing(int userId, int count);
        Task<List<dynamic>> SearchUsers(string query, int currentUserId);
        Task<(bool success, string message, int followerCount, bool isFollowing)> ToggleFollow(int currentUserId, int userId);
        Task<(bool success, string message)> AddArtwork(Tranh tranh, IFormFile imageFile, string tagsInput, List<int> selectedCategories, int currentUserId);
        Task<List<SelectListItem>> GetCategories();
    }
} 