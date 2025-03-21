using ArtGallery.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ArtGallery.Services
{
    public interface IUserCreatingHandler
    {
        void HandleUserCreating(NguoiDung user);
    }

    public class UserCreatingHandler : IUserCreatingHandler
    {
        private readonly ILogger<UserCreatingHandler> _logger;

        public UserCreatingHandler(ILogger<UserCreatingHandler> logger)
        {
            _logger = logger;
        }

        public void HandleUserCreating(NguoiDung user)
        {
            // Đảm bảo TenNguoiDung không bao giờ null
            if (string.IsNullOrEmpty(user.TenNguoiDung))
            {
                user.TenNguoiDung = user.UserName ?? user.Email?.Split('@')[0] ?? "User";
                _logger.LogInformation($"Đã thiết lập TenNguoiDung cho người dùng {user.Id} thành {user.TenNguoiDung}");
            }
        }
    }
} 