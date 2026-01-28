namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public sealed class CancelCfdiRequestDto
    {
        // 01,02,03,04
        public string Motive { get; set; } = "02";

        // requerido si Motive = 01
        public Guid? UuidReplacement { get; set; }
    }

    public sealed class CancelCfdiResultDto
    {
        public string Status { get; set; } = default!;   // canceled/requested/rejected...
        public string? Message { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? CancelationDate { get; set; }
        public string? AcuseXmlBase64 { get; set; }
    }
}
