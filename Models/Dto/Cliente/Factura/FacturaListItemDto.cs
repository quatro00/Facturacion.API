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

        public string Tipo { get; set; } = null!;      // I/E
        public string TipoNombre { get; set; } = null!; // "Factura" / "Nota de crédito"

        public string Moneda { get; set; } = null!;
        public decimal Total { get; set; }
        public string Estatus { get; set; } = null!;

        // ✅ Relación (solo aplica cuando Tipo = E)
        public Guid? CfdiOrigenId { get; set; }
        public string? OrigenUuid { get; set; }
        public string? OrigenSerie { get; set; }
        public string? OrigenFolio { get; set; }
    }
}
