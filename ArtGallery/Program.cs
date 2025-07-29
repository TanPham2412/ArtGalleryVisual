using ArtGallery.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using ArtGallery.Repositories.Interfaces;
using ArtGallery.Repositories;
using Microsoft.AspNetCore.Identity;
using ArtGallery.Data;
using Microsoft.AspNetCore.Mvc;
using ArtGallery.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using ArtGallery.Services.VNPAY;
using ArtGallery.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Loại bỏ dòng này vì đã có cấu hình connection string bên dưới
// var connectionString = builder.Configuration.GetConnectionString("ArtGallery") ?? throw new InvalidOperationException("Connection string 'ArtGallery' not found.");

builder.Services.AddHttpContextAccessor();

// Loại bỏ IAccountRepository vì đã xóa
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IArtworkRepository, ArtworkRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ITheLoaiRepositories, TheLoaiRepository>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ArtGalleryContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("ART_GALLERY", EnvironmentVariableTarget.User)));

builder.Services.AddLogging(options =>
{
    options.AddConsole();
    options.AddDebug();
    options.SetMinimumLevel(LogLevel.Debug);
});

builder.Services.AddScoped<IUserStore<NguoiDung>, CustomUserStore>();
// Đảm bảo chỉ có một cấu hình Identity
builder.Services.AddIdentity<NguoiDung, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequireDigit = false; // Không bắt buộc số
    options.Password.RequireLowercase = false; // Không bắt buộc chữ thường
    options.Password.RequireUppercase = false; // Không bắt buộc chữ hoa
    options.Password.RequireNonAlphanumeric = false; // Không bắt buộc ký tự đặc biệt
    options.Password.RequiredLength = 6; // Độ dài tối thiểu 6 ký tự
    
    options.User.RequireUniqueEmail = true; // Email phải duy nhất
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Thời gian khóa mặc định
    options.Lockout.MaxFailedAccessAttempts = 5; // Số lần thử tối đa
})
.AddEntityFrameworkStores<ArtGalleryContext>()
.AddDefaultTokenProviders();

// Cấu hình các đường dẫn Identity
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.Name = "ArtGallery.Identity";
    options.Cookie.HttpOnly = true;
});

// Thêm vào bên trong phương thức AddIdentity() hoặc sau đó
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID", EnvironmentVariableTarget.User);
        options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET", EnvironmentVariableTarget.User);
        options.CallbackPath = "/signin-google";
    });

// Thêm vào sau phần cấu hình Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    // Cấu hình khác...
});

// Phải đảm bảo đăng ký CustomUserStore trước khi đăng ký Identity

builder.Services.AddTransient<IEmailSender, EmailSender>();

// Cấu hình xử lý khi từ chối truy cập
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Artwork/AccessDenied";
    options.LoginPath = "/Home/LoginRegister";
    options.LogoutPath = "/Home/LogoutDirect";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Đăng ký NotificationRepository
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
//VNPAY
builder.Services.AddScoped<IVnPayService, VnPayService>();
// Register the email sender service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<EmailService>();

// Đăng ký dịch vụ SignalR
builder.Services.AddSignalR();


// Thêm vào phần cấu hình dịch vụ
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed Roles và Admin user
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedRolesAndAdminAsync(scope.ServiceProvider);
}

// Thêm vào phần khởi tạo dữ liệu để tạo role Artists
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "User", "Artists" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Cấu hình routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Thêm định tuyến rõ ràng cho Area Identity
app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Đảm bảo MapRazorPages được gọi (đã có)
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await ArtGallery.Data.DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi khởi tạo dữ liệu.");
    }
}

// Đảm bảo thêm dòng này vào phần cấu hình middleware
app.UseSession();

// Thêm SignalR endpoint
app.MapHub<CommentHub>("/commentHub");

app.Run();