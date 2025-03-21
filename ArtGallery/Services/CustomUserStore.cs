using System;
using System.Threading;
using System.Threading.Tasks;
using ArtGallery.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArtGallery.Services
{
    // Chỉ định rõ ArtGalleryContext thay vì DbContext
    public class CustomUserStore : UserStore<NguoiDung>
    {
        private readonly ILogger<CustomUserStore> _logger;

        public CustomUserStore(ArtGalleryContext context, ILogger<CustomUserStore> logger) 
            : base(context)
        {
            _logger = logger;
        }

        public override async Task<IdentityResult> CreateAsync(NguoiDung user, CancellationToken cancellationToken = default)
        {
            // Đảm bảo TenNguoiDung luôn có giá trị trước khi lưu
            if (string.IsNullOrEmpty(user.TenNguoiDung))
            {
                // Ưu tiên sử dụng UserName nếu có
                if (!string.IsNullOrEmpty(user.UserName))
                {
                    user.TenNguoiDung = user.UserName;
                }
                // Nếu không có UserName, tạo từ Email hoặc một giá trị mặc định
                else if (!string.IsNullOrEmpty(user.Email))
                {
                    user.TenNguoiDung = user.Email.Split('@')[0];
                }
                else
                {
                    user.TenNguoiDung = "User_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                }
                _logger.LogInformation($"Đã thiết lập TenNguoiDung: {user.TenNguoiDung}");
            }

            try
            {
                return await base.CreateAsync(user, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo người dùng");
                throw;
            }
        }
    }
} 