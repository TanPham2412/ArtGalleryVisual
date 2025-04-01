using Microsoft.AspNetCore.Mvc.Rendering;
using ArtGallery.Models;

namespace ArtGallery.Repositories.Interfaces
{
    public interface IHomeRepository
    {
        Task<List<Tranh>> GetRandomArtworksFromFollowing(string userId, int count);
        Task<List<Tranh>> GetRandomArtworksFromFollowingByCategory(string userId, int count, string category);
        Task<List<Tranh>> GetRecommendedArtworks();
        Task<List<Tranh>> GetMostLikedArtworks(int count);
        Task<List<Tranh>> GetMostLikedArtworksByCategory(int count, string category);
        Task<List<dynamic>> SearchUsers(string query, string currentUserId);
        Task<(bool success, string message, int followerCount, bool isFollowing)> ToggleFollow(string currentUserId, string userId);
        Task<(bool success, string message)> AddArtwork(Tranh tranh, IFormFile imageFile, string tagsInput, List<int> selectedCategories, string currentUserId);
        Task<List<SelectListItem>> GetCategories();
    }
} 