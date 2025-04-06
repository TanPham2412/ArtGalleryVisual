using System.ComponentModel.DataAnnotations;

public class ArtistRegistrationViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên nghệ sĩ")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên nghệ sĩ phải từ 2-100 ký tự")]
    public string TenNgheSi { get; set; }
    
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Địa chỉ phải từ 5-200 ký tự")]
    public string DiaChi { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mô tả về bản thân")]
    [MinLength(50, ErrorMessage = "Mô tả phải có ít nhất 50 ký tự")]
    public string MoTa { get; set; }
    
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string SoDienThoai { get; set; }
} 