using ArtGallery.Models.Account;
using ArtGallery.Models;
using System.Security.Claims;

public interface IAccountRepository
{
    Task<(bool success, string message, NguoiDung user)> ValidateLogin(LoginViewModel model);
    Task<(bool success, string message)> RegisterUser(RegisterViewModel model);
    Task<bool> IsUsernameExists(string username);
    Task<bool> IsEmailExists(string email);
    Task<List<Claim>> GenerateUserClaims(NguoiDung user);
    Task<(bool success, string message, NguoiDung user)> ValidateLogin1(LoginViewModel model);
    Task<NguoiDung> FindUserByEmail(string email);
    Task<NguoiDung> CreateGoogleUser(NguoiDung user);
}