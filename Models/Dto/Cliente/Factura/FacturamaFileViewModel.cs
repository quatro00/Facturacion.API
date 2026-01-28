namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public sealed class FacturamaFileViewModel
    {
        public string? ContentEncoding { get; set; } // normalmente "base64"
        public string? ContentType { get; set; }     // "pdf" | "xml" | "html"
        public long ContentLength { get; set; }
        public string? Content { get; set; }         // base64 del archivo
    }
}
