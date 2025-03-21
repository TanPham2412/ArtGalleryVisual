using ArtGallery.Models;
using Microsoft.AspNetCore.Http;

namespace ArtGallery.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<(NguoiDung user, int followersCount, int followingCount)?> GetUserProfile(string userId);
        Task<(bool success, string message)> UpdateProfile(NguoiDung model, IFormFile profileImage, IFormFile coverImage, List<string> LoaiMedia, List<string> DuongDan);
    }
}