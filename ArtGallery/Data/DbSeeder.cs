using ArtGallery.Models;
using Microsoft.AspNetCore.Identity;

namespace ArtGallery.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<NguoiDung>>();
        
        // Tạo vai trò nếu chưa tồn tại
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        
        // Kiểm tra xem đã có tài khoản admin nào chưa
        if ((await userManager.GetUsersInRoleAsync("Admin")).Count == 0)
        {
            // Tạo tài khoản Admin nếu chưa có
            var adminUser = new NguoiDung
            {
                TenNguoiDung = "Administrator",
                UserName = "Admin123",
                Email = "AdminPiaoYue@gmail.com",
                EmailConfirmed = true, // Đã xác nhận email
                PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            
            // Tạo tài khoản admin với mật khẩu
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            
            if (result.Succeeded)
            {
                // Gán vai trò Admin cho tài khoản
                await userManager.AddToRoleAsync(adminUser, "Admin");
                
                // Log thông báo
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Tài khoản Admin đã được tạo thành công!");
            }
            else
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError("Không thể tạo tài khoản Admin. Lỗi: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
} 