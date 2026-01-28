using Facturacion.API.Helpers;
using Facturacion.API.Models.Dto;
using Facturacion.API.Models.Dto.Cliente.Factura;
using Facturacion.API.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.API.Controllers.Cliente
{
    [Route("api/cliente/[controller]")]
    [ApiController]
    public class FacturaController : ControllerBase
    {
        private readonly IFacturacionService _facturacionService;

        public FacturaController(IFacturacionService facturacionService) => _facturacionService = facturacionService;

        [HttpPost("emitir-multi")]
        public async Task<IActionResult> EmitirMulti([FromBody] EmitirCfdiRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.CfdiType))
                req.CfdiType = "I";
            var cuentaId = Guid.Parse(User.GetCuentaId());
            var result = await _facturacionService.EmitirCfdiMultiAsync(req,Guid.Parse(User.GetCuentaId()), ct);
            return Ok(result.RootElement); // o mapear a tu propio DTO
        }

        [HttpGet("GetFacturas")]
        public async Task<ActionResult<PagedResult<FacturaListItemDto>>> GetFacturas(
        [FromQuery] GetFacturasQuery query,
        CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());
            var result = await _facturacionService.GetFacturasAsync(cuentaId, query, ct);
            return Ok(result);
        }
    }
}
