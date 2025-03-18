using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng đồng ý với điều khoản & điều kiện")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Vui lòng đồng ý với điều khoản & điều kiện")]
        public bool DongYDieuKhoan { get; set; }
    }
}