# 🎵 PiaoYue ArtGallery
PiaoYue ArtGallery là nền tảng thương mại và trưng bày tác phẩm nghệ thuật mã nguồn mở.

## 📝 Giới thiệu
PiaoYue ArtGallery được phát triển nhằm cung cấp một nền tảng cho các nghệ sĩ trưng bày và bán tác phẩm nghệ thuật của họ. Người dùng có thể duyệt, tìm kiếm, theo dõi nghệ sĩ yêu thích và mua các tác phẩm nghệ thuật.

## ✨ Tính năng chính
- **Quản lý tài khoản**: Đăng ký, đăng nhập, phân quyền người dùng
- **Trưng bày tác phẩm**: Nghệ sĩ có thể đăng tải và quản lý tác phẩm của mình
- **Tìm kiếm và lọc**: Tìm kiếm theo tên, thể loại, nghệ sĩ
- **Tương tác xã hội**: Theo dõi nghệ sĩ, bình luận, yêu thích tác phẩm
- **Trò chuyện**: Hệ thống tin nhắn trực tiếp giữa người dùng
- **Thông báo**: Thông báo khi có tương tác mới
- **Mua bán**: Hệ thống giỏ hàng và thanh toán qua VNPAY
- **Bình luận và đánh giá**: Hệ thống bình luận và phản hồi tương tác
- **Quản trị hệ thống (Admin)**: Quản lý người dùng, duyệt nghệ sĩ, quản lý tác phẩm và đơn hàng.

## 🛠️ Công nghệ sử dụng
- **Backend**: ASP.NET Core 8.0 MVC
- **Frontend**: HTML, CSS, JavaScript, jQuery
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Payment Gateway**: VNPAY Integration
- **Email Service**: SMTP Integration

## 🚀 Clone Project
Để lấy mã nguồn về máy tính của bạn, chạy lệnh sau trong terminal:
```bash
git clone https://github.com/TanPham2412/ArtGalleryVisual.git
```

## 🔧 Cài đặt và chạy
1. **Yêu cầu hệ thống**:
   - .NET 8.0 SDK hoặc cao hơn
   - SQL Server
   - (Tùy chọn) Client ID và Client Secret cho Google Authentication.
2. **Cấu hình môi trường**:
   - Thiết lập biến môi trường `ART_GALLERY` với chuỗi kết nối đến SQL Server của bạn.
     Ví dụ (Windows PowerShell):
     `$env:ART_GALLERY="Server=your_server;Database=your_database;User Id=your_user;Password=your_password;Trusted_Connection=False;Encrypt=True;TrustServerCertificate=True;"`
   - (Tùy chọn) Nếu bạn muốn sử dụng đăng nhập Google, thiết lập các biến môi trường sau:
     `$env:GOOGLE_CLIENT_ID="YOUR_GOOGLE_CLIENT_ID"`
     `$env:GOOGLE_CLIENT_SECRET="YOUR_GOOGLE_CLIENT_SECRET"`
     Lưu ý: CallbackPath cho Google Auth được cấu hình là `/signin-google`.
3. **Khởi tạo database**:
   Mở terminal hoặc command prompt trong thư mục `ArtGallery` của dự án:
   ```bash
   dotnet ef database update
   ```
4. **Chạy ứng dụng**:
   ```bash
   dotnet run --project ArtGallery
   ```
5. **Truy cập ứng dụng**: Mở trình duyệt và truy cập URL được hiển thị trong console khi ứng dụng khởi chạy (thường là `http://localhost:xxxx` hoặc `https://localhost:xxxx`). Bạn có thể kiểm tra các URL cấu hình trong `ArtGallery/Properties/launchSettings.json`.

## 📁 Cấu trúc dự án
- `/Areas`: Chứa các khu vực phân tách trong ứng dụng MVC
- `/Controllers`: Chứa các controller xử lý request
- `/Data`: Chứa database context (`ArtGalleryContext.cs`)
- `/Libraries`: Thư viện và tiện ích
- `/Migrations`: Database migrations được tạo bởi Entity Framework Core
- `/Models`: Các model dữ liệu (Artwork, User, Comment, Message, etc.)
- `/Properties`: Chứa cấu hình khởi chạy ứng dụng (ví dụ: `launchSettings.json`)
- `/Repositories`: Pattern repository cho truy cập dữ liệu (ArtworkRepository, UserRepository, etc.)
- `/Services`: Các dịch vụ business logic (VNPAY service, Email service)
- `/ViewModels`: Các model dùng cho view
- `/Views`: Giao diện người dùng (Razor views)
- `/wwwroot`: Tài nguyên tĩnh (CSS, JS, hình ảnh)

## 📝 Cấu trúc Models
- `Tranh.cs`: Đơn vị tác phẩm nghệ thuật
- `NguoiDung.cs`: Thông tin người dùng và nghệ sĩ
- `BinhLuan.cs`: Hệ thống bình luận
- `TinNhan.cs`: Hệ thống tin nhắn
- `TheoDoi.cs`: Theo dõi nghệ sĩ
- `LuuTranh.cs`: Lưu trữ tác phẩm yêu thích
- `ThongBao.cs`: Hệ thống thông báo
- `VNPAY`: Tích hợp thanh toán

## 👨‍💻 Đóng góp
Chúng tôi rất hoan nghênh đóng góp từ cộng đồng. Nếu bạn muốn đóng góp:
1. Fork repository
2. Tạo branch mới (`git checkout -b feature/amazing-feature`)
3. Commit thay đổi (`git commit -m 'Add some amazing feature'`)
4. Push lên branch (`git push origin feature/amazing-feature`)
5. Mở Pull Request

## 📊 RepoBeats Analytics
<p align="center">
    <img src="https://repobeats.axiom.co/api/embed/d2fd449cf12eb010947325731445c985db76b96f.svg" alt="RepoBeats analytics" />
</p>
