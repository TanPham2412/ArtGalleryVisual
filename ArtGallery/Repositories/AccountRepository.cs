using ArtGallery.Models.Account;
using ArtGallery.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

public class AccountRepository : IAccountRepository
{
    private readonly ArtGalleryContext _context;
    private readonly ILogger<AccountRepository> _logger;

    public AccountRepository(ArtGalleryContext context, ILogger<AccountRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool success, string message, NguoiDung user)> ValidateLogin(LoginViewModel model)
    {
        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u =>
                (u.TenDangNhap == model.TenDangNhap || u.Email == model.TenDangNhap) &&
                u.MatKhau == model.MatKhau);

        if (user != null)
        {
            return (true, "Đăng nhập thành công", user);
        }
        return (false, "Tên đăng nhập hoặc mật khẩu không đúng", null);
    }

    public async Task<(bool success, string message, NguoiDung user)> ValidateLogin1(LoginViewModel model)
    {
        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u =>
                (u.TenDangNhap == model.TenDangNhap || u.Email == model.TenDangNhap) &&
                u.MatKhau == model.MatKhau);

        if (user != null)
        {
            return (true, "Đăng nhập thành công", user);
        }
        return (false,"Tên đăng nhập hoặc mật khẩu không đúng", null);

    }

    public async Task<(bool success, string message)> RegisterUser(RegisterViewModel model)
    {
        if (!model.DongYDieuKhoan)
        {
            return (false, "Vui lòng đồng ý với điều khoản & điều kiện");
        }

        if (await IsUsernameExists(model.TenDangNhap))
        {
            return (false, "Tên đăng nhập đã tồn tại");
        }

        if (await IsEmailExists(model.Email))
        {
            return (false, "Email đã được sử dụng");
        }

        var nguoiDung = new NguoiDung
        {
            TenDangNhap = model.TenDangNhap,
            Email = model.Email,
            MatKhau = model.MatKhau,
            TenNguoiDung = model.TenDangNhap,
            NgayTao = DateTime.Now
        };

        _context.NguoiDungs.Add(nguoiDung);
        await _context.SaveChangesAsync();

        return (true, "Đăng ký thành công");
    }

    public async Task<bool> IsUsernameExists(string username)
    {
        return await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == username);
    }

    public async Task<bool> IsEmailExists(string email)
    {
        return await _context.NguoiDungs.AnyAsync(u => u.Email == email);
    }

    public async Task<List<Claim>> GenerateUserClaims(NguoiDung user)
    {
        return new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.TenDangNhap),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("UserId", user.MaNguoiDung.ToString())
        };
    }

    public async Task<NguoiDung> FindUserByEmail(string email)
    {
        return await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<NguoiDung> CreateGoogleUser(NguoiDung user)
    {
        try
        {
            // Kiểm tra xem tên đăng nhập đã tồn tại chưa
            if (await IsUsernameExists(user.TenDangNhap))
            {
                // Nếu tên đăng nhập đã tồn tại, thêm số ngẫu nhiên vào cuối
                var random = new Random();
                user.TenDangNhap = $"{user.TenDangNhap}{random.Next(1000, 9999)}";
            }

            // Tạo mật khẩu ngẫu nhiên cho user Google
            user.MatKhau = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());
            
            _context.NguoiDungs.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo tài khoản Google cho user {Email}", user.Email);
            throw;
        }
    }
}