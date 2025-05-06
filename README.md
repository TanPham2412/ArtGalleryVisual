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
- **Mua bán**: Hệ thống giỏ hàng và thanh toán

## 🛠️ Công nghệ sử dụng
- **Backend**: ASP.NET Core MVC
- **Frontend**: HTML, CSS, JavaScript, jQuery
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Identity

## 🚀 Clone Project
Để lấy mã nguồn về máy tính của bạn, chạy lệnh sau trong terminal:
```bash
git clone https://github.com/TanPham2412/ArtGalleryVisual.git
```
## 🔧 Cài đặt và chạy
1. **Yêu cầu hệ thống**:
   - .NET 7.0 SDK hoặc cao hơn
   - SQL Server
2. **Cấu hình database**:
   - Mở file `appsettings.json`
   - Cập nhật chuỗi kết nối database
3. **Khởi tạo database**:
   ```bash
   dotnet ef database update
   ```
4. **Chạy ứng dụng**:
   ```bash
   dotnet run
   ```
5. **Truy cập ứng dụng**: Mở trình duyệt và truy cập `http://localhost:5000`

## 📁 Cấu trúc dự án
- `/Areas`: Chứa các khu vực phân tách trong ứng dụng MVC
- `/Controllers`: Chứa các controller xử lý request
- `/Data`: Chứa database context và migration
- `/Extensions`: Các phương thức mở rộng
- `/Libraries`: Thư viện và tiện ích
- `/Migrations`: Database migrations
- `/Models`: Các model dữ liệu
- `/Repositories`: Pattern repository cho truy cập dữ liệu
- `/Services`: Các dịch vụ business logic
- `/ViewModels`: Các model dùng cho view
- `/Views`: Giao diện người dùng
- `/wwwroot`: Tài nguyên tĩnh (CSS, JS, hình ảnh)

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
