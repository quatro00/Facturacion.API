namespace Facturacion.API.Services.Interface
{
    public interface IEmailSender
    {
        Task<string?> SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        IReadOnlyList<(byte[] bytes, string filename, string contentType)> attachments,
        CancellationToken ct = default);
    }
}
