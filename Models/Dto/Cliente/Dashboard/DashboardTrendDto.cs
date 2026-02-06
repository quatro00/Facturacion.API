namespace Facturacion.API.Models.Dto.Cliente.Dashboard
{
    public sealed class DashboardTrendDto
    {
        public string Direction { get; set; } = "up"; // "up" | "down"
        public string Value { get; set; } = default!; // "+12.4%"
        public string Note { get; set; } = default!;  // "vs. periodo anterior"
    }
}
