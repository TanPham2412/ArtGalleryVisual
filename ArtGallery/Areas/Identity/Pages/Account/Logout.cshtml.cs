// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ArtGallery.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ArtGallery.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<NguoiDung> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult OnGet(string returnUrl = null)
        {
            // Nếu được gọi trực tiếp từ URL, tự động đăng xuất
            return SignOut();
        }

        private IActionResult SignOut()
        {
            // Đăng xuất và chuyển hướng
            _signInManager.SignOutAsync();
            return Redirect("/Account/Login_register");
        }
    }
}
