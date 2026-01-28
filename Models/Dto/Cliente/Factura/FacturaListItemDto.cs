namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public class FacturaListItemDto
    {
        public Guid Id { get; set; }
        public string FacturamaId { get; set; } = null!;
        public string Uuid { get; set; } = null!;

        public DateTime Fecha { get; set; }

        public string? Serie { get; set; }
        public string? Folio { get; set; }

        public string ReceptorRfc { get; set; } = null!;
        public string ReceptorNombre { get; set; } = null!;

        public string Tipo { get; set; } = null!;
        public string Moneda { get; set; } = null!;

        public decimal Total { get; set; }
        public string Estatus { get; set; } = null!;
    }
}
