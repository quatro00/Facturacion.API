namespace Facturacion.API.Models.Dto.Cliente.Dashboard
{
    public sealed class DashboardRecentCfdiDto
    {
        public Guid Id { get; set; }
        public string Uuid { get; set; } = default!;
        public DateTime Fecha { get; set; }

        public string Serie { get; set; } = default!;
        public string Folio { get; set; } = default!;

        public string Receptor { get; set; } = default!;
        public string Rfc { get; set; } = default!;

        public decimal Total { get; set; }
        public string Moneda { get; set; } = "MXN";

        public string Estatus { get; set; } = default!; // TIMBRADO/CANCELADO/...
        public string? TipoCfdi { get; set; }           // I/E/P (opcional, útil)
    }
}
