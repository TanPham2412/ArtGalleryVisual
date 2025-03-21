using ArtGallery.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ArtGallery.Services
{
    public class CustomUserValidator : IUserValidator<NguoiDung>
    {
        private readonly IUserCreatingHandler _userCreatingHandler;
        private readonly ILogger<CustomUserValidator> _logger;

        public CustomUserValidator(
            IUserCreatingHandler userCreatingHandler,
            ILogger<CustomUserValidator> logger)
        {
            _userCreatingHandler = userCreatingHandler;
            _logger = logger;
        }

        public Task<IdentityResult> ValidateAsync(UserManager<NguoiDung> manager, NguoiDung user)
        {
            try
            {
                _logger.LogInformation($"Đang xác thực người dùng {user.UserName}");
                _userCreatingHandler.HandleUserCreating(user);
                return Task.FromResult(IdentityResult.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác thực người dùng");
                return Task.FromResult(IdentityResult.Failed(new IdentityError 
                { 
                    Description = $"Lỗi xác thực: {ex.Message}" 
                }));
            }
        }
    }
} 