using Facturacion.API.Helpers;
using Facturacion.API.Models.Dto;
using Facturacion.API.Models.Dto.Cliente.Factura;
using Facturacion.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = "Cliente")]
        [HttpPost("emitir-multi")]
        public async Task<IActionResult> EmitirMulti([FromBody] EmitirCfdiRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.CfdiType))
                req.CfdiType = "I";
            var cuentaId = Guid.Parse(User.GetCuentaId());
            var result = await _facturacionService.EmitirCfdiMultiAsync(req,Guid.Parse(User.GetCuentaId()), ct);
            return Ok(result.RootElement); // o mapear a tu propio DTO
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("GetFacturas")]
        public async Task<ActionResult<PagedResult<FacturaListItemDto>>> GetFacturas(
        [FromQuery] GetFacturasQuery query,
        CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());
            var result = await _facturacionService.GetFacturasAsync(cuentaId, query, ct);
            return Ok(result);
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("{id}/xml")]
        public async Task<IActionResult> DownloadXml(string id, [FromQuery] string type = "issuedLite", CancellationToken ct = default)
        {
            type = "issuedLite";
            var (bytes, filename, contentType) = await _facturacionService.GetXmlAsync(id, type, ct);
            return File(bytes, contentType, filename);
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadPdf(string id, [FromQuery] string type = "issuedLite", CancellationToken ct = default)
        {
            type = "issuedLite";
            var (bytes, filename, contentType) = await _facturacionService.GetPdfAsync(id, type, ct);
            return File(bytes, contentType, filename);
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("{id}/zip")]
        public async Task<IActionResult> DownloadZip(string id, [FromQuery] string type = "issuedLite", CancellationToken ct = default)
        {
            var (bytes, filename, contentType) = await _facturacionService.GetZipAsync(id, type, ct);
            return File(bytes, contentType, filename);
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancelar(Guid id, [FromBody] CancelCfdiRequestDto req, CancellationToken ct)
        {
            // cuentaId: sácalo del JWT/claims (o como lo manejes)
            var cuentaId = Guid.Parse(User.GetCuentaId());

            var result = await _facturacionService.CancelarCfdiAsync(id, cuentaId, req, ct);
            return Ok(result);
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("{id:guid}/acuse")]
        public async Task<IActionResult> DescargarAcuse(Guid id, CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());

            var (bytes, filename, contentType) =
                await _facturacionService.GetAcuseCancelacionAsync(id, cuentaId, ct);

            //Response.Headers["Content-Disposition"] = $"attachment; filename=\"{filename}\"";
            return File(bytes, contentType, filename);
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost("{id:guid}/reenviar")]
        public async Task<IActionResult> Reenviar(Guid id, [FromBody] ReenviarCfdiRequestDto req, CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());
            var result = await _facturacionService.ReenviarCfdiAsync(id, cuentaId, req, ct);
            return Ok(result);
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDetalle(Guid id, CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());
            var dto = await _facturacionService.GetCfdiDetalleAsync(id, cuentaId, ct);
            return Ok(dto);
        }
    }
}
