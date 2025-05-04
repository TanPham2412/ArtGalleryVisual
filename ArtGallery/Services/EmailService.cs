using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;

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

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_emailSettings.FromEmail, _emailSettings.AppPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

}
