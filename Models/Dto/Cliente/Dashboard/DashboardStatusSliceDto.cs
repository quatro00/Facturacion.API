namespace Facturacion.API.Models.Dto.Cliente.Dashboard
{
    public sealed class DashboardStatusSliceDto
    {
        // "TIMBRADO", "CANCELADO", "BORRADOR", "ERROR"
        public string Status { get; set; } = default!;
        public string Label { get; set; } = default!;
        public int Value { get; set; } // porcentaje 0-100
    }
}
