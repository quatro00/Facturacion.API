namespace Facturacion.API.Models.Dto.Cliente.Dashboard
{
    public sealed class DashboardTopClientDto
    {
        public Guid? ClienteId { get; set; } // opcional
        public string Nombre { get; set; } = default!;
        public string Rfc { get; set; } = default!;
        public int Facturas { get; set; }
        public decimal Total { get; set; }
    }
}
