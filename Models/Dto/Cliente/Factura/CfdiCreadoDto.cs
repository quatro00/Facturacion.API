namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public class CfdiCreadoDto
    {
        public Guid Id { get; init; }
        public string? Uuid { get; init; }
        public string? FacturamaId { get; init; }
        public string? Serie { get; init; }
        public string? Folio { get; init; }
        public decimal Total { get; init; }
    }
}
