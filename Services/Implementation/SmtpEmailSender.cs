
using Facturacion.API.Services.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Facturacion.API.Services.Implementation
{
    public sealed class EmailOptions
    {
        public string FromName { get; set; } = "Facturación";
        public string FromEmail { get; set; } = default!;

        public string SmtpHost { get; set; } = default!;
        public int SmtpPort { get; set; } = 587;
        public bool UseSsl { get; set; } = false;       // 465
        public bool UseStartTls { get; set; } = true;   // 587

        public string User { get; set; } = default!;
        public string Pass { get; set; } = default!;
    }
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _opt;

        public SmtpEmailSender(IOptions<EmailOptions> opt)
        {
            _opt = opt.Value;
        }

        public async Task<string?> SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        IReadOnlyList<(byte[] bytes, string filename, string contentType)> attachments,
        CancellationToken ct = default)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_opt.FromName, _opt.FromEmail));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };

            foreach (var a in attachments)
                builder.Attachments.Add(a.filename, a.bytes, ContentType.Parse(a.contentType));

            msg.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            var secure = _opt.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : (_opt.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

            await smtp.ConnectAsync(_opt.SmtpHost, _opt.SmtpPort, secure, ct);
            await smtp.AuthenticateAsync(_opt.User, _opt.Pass, ct);

            var resp = await smtp.SendAsync(msg, ct);
            await smtp.DisconnectAsync(true, ct);

            return resp;
        }
    }
}
