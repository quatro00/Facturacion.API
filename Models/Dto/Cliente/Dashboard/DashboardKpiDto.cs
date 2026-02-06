namespace Facturacion.API.Models.Dto.Cliente.Dashboard
{
    public sealed class DashboardKpiDto
    {
        public string Title { get; set; } = default!;
        public string Value { get; set; } = default!;
        public string Hint { get; set; } = default!;
        public string Icon { get; set; } = default!; // "paid", "receipt_long", etc.

        public DashboardTrendDto? Trend { get; set; }
    }
}
