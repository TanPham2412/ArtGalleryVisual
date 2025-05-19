using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ArtGallery.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ArtGallery.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly SignInManager<NguoiDung> _signInManager;

        public IndexModel(
            UserManager<NguoiDung> userManager,
            SignInManager<NguoiDung> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }
        
        public string CurrentAvatar { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Họ tên")]
            [Required(ErrorMessage = "Vui lòng nhập họ tên")]
            [StringLength(100, ErrorMessage = "Họ tên phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 2)]
            public string TenNguoiDung { get; set; }
            
            [Phone]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }
            
            [Display(Name = "Ảnh đại diện")]
            public IFormFile AnhDaiDien { get; set; }
            
            [Display(Name = "Địa chỉ")]
            public string DiaChi { get; set; }
        }

        private async Task LoadAsync(NguoiDung user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            CurrentAvatar = user.GetAvatarPath();

            Input = new InputModel
            {
                TenNguoiDung = user.TenNguoiDung,
                PhoneNumber = phoneNumber,
                DiaChi = user.DiaChi
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không thể tải thông tin người dùng với ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không thể tải thông tin người dùng với ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Lỗi khi cập nhật số điện thoại.";
                    return RedirectToPage();
                }
            }
            
            if (Input.TenNguoiDung != user.TenNguoiDung)
            {
                user.TenNguoiDung = Input.TenNguoiDung;
            }
            
            if (Input.DiaChi != user.DiaChi)
            {
                user.DiaChi = Input.DiaChi;
            }
            
            if (Input.AnhDaiDien != null && Input.AnhDaiDien.Length > 0)
            {
                // Thêm logic lưu file ảnh vào thư mục và cập nhật đường dẫn trong database
                var fileName = $"{user.Id}_{DateTime.Now.Ticks}{Path.GetExtension(Input.AnhDaiDien.FileName)}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users", fileName);
                
                // Đảm bảo thư mục tồn tại
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.AnhDaiDien.CopyToAsync(stream);
                }
                
                user.AnhDaiDien = $"/images/users/{fileName}";
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Thông tin hồ sơ của bạn đã được cập nhật";
            return RedirectToPage();
        }
    }
} 