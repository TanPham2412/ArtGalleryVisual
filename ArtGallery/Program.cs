using ArtGallery.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using ArtGallery.Repositories.Interfaces;
using ArtGallery.Repositories;
using Microsoft.AspNetCore.Authentication.Google;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IArtworkRepository, ArtworkRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ArtGalleryContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("ART_GALLERY", EnvironmentVariableTarget.User)));

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    })
    .AddGoogle(options =>
    {
        options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID", EnvironmentVariableTarget.User);
        options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET", EnvironmentVariableTarget.User);
        options.CallbackPath = "/signin-google";
    });



var app = builder.Build();
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
app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();