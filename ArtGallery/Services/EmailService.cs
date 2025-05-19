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
        email.Subject = "Xác nhận đơn hàng - PiaoYue Art Gallery";

        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Xác nhận đơn hàng</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333333;
                        background-color: #f9f9f9;
                        margin: 0;
                        padding: 0;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background: #ffffff;
                        border-radius: 12px;
                        box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                    }}
                    .header {{
                        text-align: center;
                        padding: 20px 0;
                        background: linear-gradient(135deg, #001e36 0%, #0046ad 100%);
                        border-radius: 8px 8px 0 0;
                        margin-bottom: 20px;
                    }}
                    .header h1 {{
                        color: white;
                        margin: 0;
                        padding: 0;
                        font-size: 24px;
                    }}
                    .gallery-name {{
                        color: #00ccff;
                        font-weight: bold;
                    }}
                    .content {{
                        padding: 20px;
                    }}
                    .order-info {{
                        background-color: #f5f8ff;
                        padding: 15px;
                        border-radius: 8px;
                        margin: 15px 0;
                        border-left: 4px solid #0046ad;
                    }}
                    .footer {{
                        text-align: center;
                        margin-top: 20px;
                        padding-top: 20px;
                        border-top: 1px solid #eeeeee;
                        color: #888888;
                        font-size: 12px;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 10px 20px;
                        background: linear-gradient(135deg, #0046ad 0%, #00ccff 100%);
                        color: white;
                        text-decoration: none;
                        border-radius: 30px;
                        font-weight: bold;
                        margin: 20px 0;
                        text-align: center;
                    }}
                    h2, h3 {{
                        color: #0046ad;
                    }}
                    .customer-info {{
                        margin-bottom: 20px;
                    }}
                    .info-label {{
                        font-weight: bold;
                        color: #555555;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1><span class='gallery-name'>PiaoYue</span> Art Gallery</h1>
                    </div>
                    <div class='content'>
                        <h2>Chào {userName},</h2>
                        <p>Bạn đã đặt hàng thành công tại <strong>PiaoYue Art Gallery</strong>.</p>
                        
                        <h3>Thông tin đơn hàng:</h3>
                        <div class='order-info'>
                            {orderInfo}
                        </div>
                        
                        <p>Đơn hàng của bạn đang được xử lý. Chúng tôi sẽ thông báo cho bạn khi đơn hàng được gửi đi.</p>
                        
                        <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi.</p>
                        
                        <p>Cảm ơn bạn đã mua sắm!</p>
                    </div>
                    <div class='footer'>
                        <p>© 2023 PiaoYue Art Gallery. Tất cả các quyền được bảo lưu.</p>
                        <p>Email này được gửi tự động, vui lòng không phản hồi.</p>
                    </div>
                </div>
            </body>
            </html>"
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
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Xác nhận Email</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333333;
                        background-color: #f9f9f9;
                        margin: 0;
                        padding: 0;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background: #1e1e1e;
                        border-radius: 12px;
                        box-shadow: 0 2px 10px rgba(0,0,0,0.2);
                        color: #e0e0e0;
                    }}
                    .header {{
                        text-align: center;
                        padding: 20px 0;
                        margin-bottom: 20px;
                        border-bottom: 1px solid #333;
                    }}
                    .header h1 {{
                        margin: 0;
                        padding: 0;
                        font-size: 24px;
                        background: linear-gradient(to right, #00a8ff, #9c27b0);
                        -webkit-background-clip: text;
                        -webkit-text-fill-color: transparent;
                        display: inline-block;
                    }}
                    .content {{
                        padding: 20px;
                    }}
                    .footer {{
                        text-align: center;
                        margin-top: 20px;
                        padding-top: 20px;
                        border-top: 1px solid #333;
                        color: #888888;
                        font-size: 12px;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 12px 30px;
                        background: linear-gradient(45deg, #00a8ff, #9c27b0);
                        color: white;
                        text-decoration: none;
                        border-radius: 30px;
                        font-weight: bold;
                        margin: 20px 0;
                        text-align: center;
                        box-shadow: 0 4px 10px rgba(0, 168, 255, 0.3);
                        transition: all 0.3s ease;
                    }}
                    .button:hover {{
                        transform: translateY(-2px);
                        box-shadow: 0 6px 15px rgba(0, 168, 255, 0.4);
                    }}
                    h2, h3 {{
                        color: #e0e0e0;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>PiaoYue Art Gallery</h1>
                    </div>
                    <div class='content'>
                        <h2>Xin chào {userName},</h2>
                        <p>Cảm ơn bạn đã cập nhật địa chỉ email của mình tại <strong>PiaoYue Art Gallery</strong>.</p>
                        
                        <p>Để xác nhận địa chỉ email mới của bạn, vui lòng nhấp vào nút bên dưới:</p>
                        
                        <div style='text-align: center;'>
                            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='button'>Xác nhận email</a>
                        </div>
                        
                        <p>Nếu bạn không thực hiện thay đổi này, vui lòng bỏ qua email này.</p>
                        
                        <p>Trân trọng,<br/>Đội ngũ PiaoYue Art Gallery</p>
                    </div>
                    <div class='footer'>
                        <p>© 2023 PiaoYue Art Gallery. Tất cả các quyền được bảo lưu.</p>
                        <p>Email này được gửi tự động, vui lòng không phản hồi.</p>
                    </div>
                </div>
            </body>
            </html>"
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
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Xác minh Email</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333333;
                        background-color: #f9f9f9;
                        margin: 0;
                        padding: 0;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background: #1e1e1e;
                        border-radius: 12px;
                        box-shadow: 0 2px 10px rgba(0,0,0,0.2);
                        color: #e0e0e0;
                    }}
                    .header {{
                        text-align: center;
                        padding: 20px 0;
                        margin-bottom: 20px;
                        border-bottom: 1px solid #333;
                    }}
                    .header h1 {{
                        margin: 0;
                        padding: 0;
                        font-size: 24px;
                        background: linear-gradient(to right, #00a8ff, #9c27b0);
                        -webkit-background-clip: text;
                        -webkit-text-fill-color: transparent;
                        display: inline-block;
                    }}
                    .content {{
                        padding: 20px;
                    }}
                    .footer {{
                        text-align: center;
                        margin-top: 20px;
                        padding-top: 20px;
                        border-top: 1px solid #333;
                        color: #888888;
                        font-size: 12px;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 12px 30px;
                        background: linear-gradient(45deg, #00a8ff, #9c27b0);
                        color: white;
                        text-decoration: none;
                        border-radius: 30px;
                        font-weight: bold;
                        margin: 20px 0;
                        text-align: center;
                        box-shadow: 0 4px 10px rgba(0, 168, 255, 0.3);
                        transition: all 0.3s ease;
                    }}
                    .button:hover {{
                        transform: translateY(-2px);
                        box-shadow: 0 6px 15px rgba(0, 168, 255, 0.4);
                    }}
                    h2, h3 {{
                        color: #e0e0e0;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>PiaoYue Art Gallery</h1>
                    </div>
                    <div class='content'>
                        <h2>Xin chào {userName},</h2>
                        <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>PiaoYue Art Gallery</strong>.</p>
                        
                        <p>Vui lòng xác minh địa chỉ email của bạn bằng cách nhấp vào nút bên dưới:</p>
                        
                        <div style='text-align: center;'>
                            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='button'>Xác minh email</a>
                        </div>
                        
                        <p>Nếu bạn không yêu cầu xác minh email này, vui lòng bỏ qua email này.</p>
                        
                        <p>Trân trọng,<br/>Đội ngũ PiaoYue Art Gallery</p>
                    </div>
                    <div class='footer'>
                        <p>© 2023 PiaoYue Art Gallery. Tất cả các quyền được bảo lưu.</p>
                        <p>Email này được gửi tự động, vui lòng không phản hồi.</p>
                    </div>
                </div>
            </body>
            </html>"
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
