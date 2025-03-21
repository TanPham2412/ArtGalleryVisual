using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ArtGallery.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ArtGallery.Areas.Identity.Pages.Account
{
    public class CustomExternalLoginHandler
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly ILogger<CustomExternalLoginHandler> _logger;

        public CustomExternalLoginHandler(
            UserManager<NguoiDung> userManager,
            ILogger<CustomExternalLoginHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<NguoiDung> HandleExternalLoginAsync(ExternalLoginInfo info)
        {
            // Tìm người dùng theo email từ thông tin xác thực bên ngoài
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Tạo người dùng mới nếu chưa tồn tại
                user = new NguoiDung
                {
                    Id = Guid.NewGuid().ToString(), // Đặt ID trước khi thêm vào database
                    UserName = email,
                    Email = email,
                    TenNguoiDung = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0],
                    NgayTao = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Lỗi khi tạo người dùng từ Google: {Errors}", errors);
                    throw new Exception($"Lỗi khi tạo người dùng: {errors}");
                }

                // Thêm login cho người dùng
                result = await _userManager.AddLoginAsync(user, info);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Lỗi khi thêm login từ Google: {Errors}", errors);
                    throw new Exception($"Lỗi khi thêm login: {errors}");
                }

                // Thêm người dùng vào role "User"
                await _userManager.AddToRoleAsync(user, "User");
            }

            return user;
        }
    }
} 