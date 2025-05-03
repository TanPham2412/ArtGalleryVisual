using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

public class EmailService
{
    private readonly string senderEmail;
    private readonly string senderName;
    private readonly string appPassword;

    // Tiêm các giá trị cấu hình qua constructor
    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        senderEmail = emailSettings.Value.SenderEmail;
        senderName = emailSettings.Value.SenderName;
        appPassword = emailSettings.Value.AppPassword;
    }

    public async Task SendOrderConfirmationEmail(string toEmail, string toName, string subject, string body)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(senderName, senderEmail));
            email.To.Add(new MailboxAddress(toName, toEmail));
            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = body
            };

            // Kết nối tới máy chủ SMTP của Gmail
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(senderEmail, appPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Xử lý ngoại lệ, có thể log hoặc báo lỗi
            throw new Exception("Có lỗi khi gửi email: " + ex.Message);
        }
    }
}
