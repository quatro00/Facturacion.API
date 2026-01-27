namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public class FacturamaCsdRequest
    {
        public string Rfc { get; set; } = null!;
        public string Certificate { get; set; } = null!;         // Base64 del .cer
        public string PrivateKey { get; set; } = null!;          // Base64 del .key
        public string PrivateKeyPassword { get; set; } = null!;  // Password en texto
    }
}
