using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Text.Encodings.Web;

public class EmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _emailSettings = options.Value;
    }

    public async Task SendOrderConfirmationEmail(string toEmail, string userName, string orderInfo)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.FromEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = "Xác nhận đơn hàng - ArtGallery";

        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = $@"
                <h3>Chào {userName},</h3>
                <p>Bạn đã đặt hàng thành công tại <strong>ArtGallery</strong>.</p>
                <p>Thông tin đơn hàng:</p>
                {orderInfo}
                <p>Cảm ơn bạn đã mua sắm!</p>"
        };

        await SendEmailAsync(email);
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string userName, string callbackUrl)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.FromEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = "Xác nhận email - PiaoYue Art Gallery";

        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = $@"
                <div style='background-color: #1e1e1e; color: #e0e0e0; padding: 20px; border-radius: 10px; font-family: Arial, sans-serif;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <h2 style='color: #00a8ff;'>PiaoYue Art Gallery</h2>
                    </div>
                    <h3 style='color: #ffffff;'>Xin chào {userName},</h3>
                    <p>Cảm ơn bạn đã cập nhật địa chỉ email của mình tại <strong>PiaoYue Art Gallery</strong>.</p>
                    <p>Để xác nhận địa chỉ email mới của bạn, vui lòng nhấp vào nút bên dưới:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                           style='background: linear-gradient(45deg, #00a8ff, #9c27b0); color: white; padding: 12px 24px; text-decoration: none; border-radius: 30px; font-weight: bold; display: inline-block;'>
                            Xác nhận email
                        </a>
                    </div>
                    <p>Nếu bạn không thực hiện thay đổi này, vui lòng bỏ qua email này.</p>
                    <p>Trân trọng,<br/>Đội ngũ PiaoYue Art Gallery</p>
                    <div style='border-top: 1px solid #333; margin-top: 20px; padding-top: 20px; font-size: 12px; color: #999; text-align: center;'>
                        <p>Email này được gửi tự động, vui lòng không phản hồi.</p>
                    </div>
                </div>"
        };

        await SendEmailAsync(email);
    }

    public async Task SendEmailVerificationAsync(string toEmail, string userName, string callbackUrl)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.FromEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = "Xác minh email - PiaoYue Art Gallery";

        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = $@"
                <div style='background-color: #1e1e1e; color: #e0e0e0; padding: 20px; border-radius: 10px; font-family: Arial, sans-serif;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <h2 style='color: #00a8ff;'>PiaoYue Art Gallery</h2>
                    </div>
                    <h3 style='color: #ffffff;'>Xin chào {userName},</h3>
                    <p>Vui lòng xác minh địa chỉ email của bạn bằng cách nhấp vào nút bên dưới:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                           style='background: linear-gradient(45deg, #00a8ff, #9c27b0); color: white; padding: 12px 24px; text-decoration: none; border-radius: 30px; font-weight: bold; display: inline-block;'>
                            Xác minh email
                        </a>
                    </div>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                    <p>Trân trọng,<br/>Đội ngũ PiaoYue Art Gallery</p>
                    <div style='border-top: 1px solid #333; margin-top: 20px; padding-top: 20px; font-size: 12px; color: #999; text-align: center;'>
                        <p>Email này được gửi tự động, vui lòng không phản hồi.</p>
                    </div>
                </div>"
        };

        await SendEmailAsync(email);
    }

    private async Task SendEmailAsync(MimeMessage email)
    {
        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, 
                _emailSettings.EnableSSL ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.Auto);
            await smtp.AuthenticateAsync(_emailSettings.FromEmail, _emailSettings.AppPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Log lỗi ở đây
            throw new Exception($"Không thể gửi email: {ex.Message}", ex);
        }
    }
}
