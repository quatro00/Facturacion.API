using Facturacion.API.Helpers;
using Facturacion.API.Models.Dto.Cliente.Dashboard;
using Facturacion.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.API.Controllers.Cliente
{
    [Route("api/Cliente/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost("GetDashboard")]
        public async Task<ActionResult<DashboardDto>> Dashboard(
        [FromBody] DashboardRequest req,
        CancellationToken ct)
        {
            if (req is null)
                return BadRequest(new { message = "Request inválido." });

            // Defaults (si no mandas nada, últimos 30 días)
            var hasta = req.Hasta ?? DateTime.UtcNow;
            var desde = req.Desde ?? hasta.AddDays(-30);

            if (desde > hasta)
                return BadRequest(new { message = "El rango de fechas es inválido: 'Desde' no puede ser mayor que 'Hasta'." });

            // normaliza en el request para el service
            req.Desde = desde;
            req.Hasta = hasta;

            var cuentaId = Guid.Parse(User.GetCuentaId());

            var dto = await _dashboardService.GetDashboardAsync(cuentaId, req, ct);
            return Ok(dto);
        }
    }
}
