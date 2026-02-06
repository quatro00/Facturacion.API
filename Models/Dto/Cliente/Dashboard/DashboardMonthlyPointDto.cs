namespace Facturacion.API.Models.Dto.Cliente.Dashboard
{
    public sealed class DashboardMonthlyPointDto
    {
        public int Year { get; set; }          // 2026
        public int Month { get; set; }         // 1..12
        public string Label { get; set; } = default!; // "Ene"
        public decimal Amount { get; set; }    // total del mes
    }
}
