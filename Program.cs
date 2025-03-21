using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Thêm CustomUserStore sau (để đảm bảo TenNguoiDung luôn có giá trị)
builder.Services.AddScoped<IUserStore<NguoiDung>, CustomUserStore>();

// Chỉ giữ một cấu hình Identity duy nhất
builder.Services.AddIdentity<NguoiDung, IdentityRole>(options => {
    // Cấu hình...
})
.AddEntityFrameworkStores<ArtGalleryContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run(); 