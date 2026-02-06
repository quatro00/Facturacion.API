namespace Facturacion.API.Models.Dto.Cliente.Dashboard
{
    public sealed class DashboardAlertDto
    {
        public string Icon { get; set; } = default!;  // "warning", "sync_problem", etc.
        public string Title { get; set; } = default!;
        public string Desc { get; set; } = default!;
        public string Severity { get; set; } = "info"; // "info" | "warn" | "danger" (opcional)
    }
}
