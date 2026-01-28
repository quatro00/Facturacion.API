namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public sealed class ReenviarCfdiRequestDto
    {
        public string? EmailTo { get; set; } // si null, usar correo del cliente
        public bool IncludeXml { get; set; } = true;
        public bool IncludePdf { get; set; } = true;
        public bool IncludeAcuseCancelacion { get; set; } = true;

        public string? Subject { get; set; }
        public string? Message { get; set; }
    }

    public sealed class ReenviarCfdiResponseDto
    {
        public bool Sent { get; set; }
        public string To { get; set; } = default!;
        public string? ProviderMessageId { get; set; }
    }
}
