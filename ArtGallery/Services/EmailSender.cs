using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace ArtGallery.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Hiện tại chỉ trả về Task.CompletedTask để hệ thống không báo lỗi
            // Bạn có thể triển khai gửi email thực tế sau này
            return Task.CompletedTask;
        }
    }
} 