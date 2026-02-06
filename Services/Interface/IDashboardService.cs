using Facturacion.API.Models.Dto.Cliente.Dashboard;

namespace Facturacion.API.Services.Interface
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync(Guid cuentaId, DashboardRequest req, CancellationToken ct);
    }
}
