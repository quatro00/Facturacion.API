namespace Facturacion.API.Models.Dto.Cliente.Dashboard
{
    public sealed class DashboardDto
    {
        public string PeriodoLabel { get; set; } = "Últimos 30 días";
        public DateTime LastSync { get; set; } = DateTime.UtcNow;

        public List<DashboardKpiDto> Kpis { get; set; } = new();
        public List<DashboardMonthlyPointDto> Facturacion12m { get; set; } = new();
        public List<DashboardStatusSliceDto> StatusSlices { get; set; } = new();

        public List<DashboardRecentCfdiDto> RecentCfdis { get; set; } = new();
        public List<DashboardTopClientDto> TopClients { get; set; } = new();

        public List<DashboardAlertDto> Alerts { get; set; } = new();
    }
}
