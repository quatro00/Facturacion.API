using Facturacion.API.Helpers;
using Facturacion.API.Models.Dto;
using Facturacion.API.Models.Dto.Cliente.Factura;
using Facturacion.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.API.Controllers.Cliente
{
    [Authorize(Roles = "Cliente")]
    [Route("api/cliente/[controller]")]
    [ApiController]
    public class FacturaController : ControllerBase
    {
        private readonly IFacturacionService _facturacionService;

        public FacturaController(IFacturacionService facturacionService) => _facturacionService = facturacionService;

        [Authorize(Roles = "Cliente")]
        [HttpPost("emitir")]
        public async Task<IActionResult> EmitirMulti([FromBody] EmitirCfdiRequest req, CancellationToken ct)
        {
            if (req is null) return BadRequest("Request inválido.");
            if (req.RazonSocialId == Guid.Empty) return BadRequest("RazonSocialId es requerido.");
            if (req.ClienteId == Guid.Empty) return BadRequest("ClienteId es requerido.");
            if (req.SucursalId == Guid.Empty) return BadRequest("SucursalId es requerido.");
            if (req.Items is null || req.Items.Count == 0) return BadRequest("Debe incluir al menos un concepto.");

            // ✅ Siempre Ingreso
            req.CfdiType = "I";

            // ✅ Normaliza y valida TipoFactura (Concepto)
            req.TipoFactura = (req.TipoFactura ?? "").Trim().ToUpperInvariant();
            if (req.TipoFactura is not ("I_MERCANCIAS" or "I_SERVICIOS" or "I_ANTICIPO"))
            {
                return BadRequest("TipoFactura inválido. Valores permitidos: MERCANCIAS | SERVICIOS | ANTICIPO.");
            }

            var cuentaId = Guid.Parse(User.GetCuentaId());

            var result = await _facturacionService.EmitirCfdiMultiAsync(req, cuentaId, ct);

            return Ok(result.RootElement);
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

        [Authorize(Roles = "Cliente")]
        [HttpPost("{cfdiId:guid}/notas-credito/total")]
        public async Task<IActionResult> CrearNcTotal(Guid cfdiId, [FromBody] CrearNcTotalRequest req, CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());
            var dto = await _facturacionService.CrearNotaCreditoTotalAsync(cuentaId, req.RazonSocialId, cfdiId, ct);
            return Ok(dto);
        }

        [HttpPost("notas-credito/parcial-monto")]
        public async Task<IActionResult> CrearNcParcialMonto(
    [FromBody] NotaCreditoParcialMontoRequest req,
    CancellationToken ct)
        {
            if (req is null)
                return BadRequest(new { message = "Request inválido." });

            if (req.Monto <= 0)
                return BadRequest(new { message = "El monto debe ser mayor a 0." });

            var cuentaId = Guid.Parse(User.GetCuentaId());

            var dto = await _facturacionService.CrearNotaCreditoParcialMontoAsync(
                cuentaId: cuentaId,
                req,
                ct: ct);

            return Ok(dto);
        }

        [HttpPost("CrearNotaCreditoDevolucion")]
        public async Task<IActionResult> CrearNotaCreditoDevolucion(
    [FromBody] CrearNcDevolucionRequest req,
    CancellationToken ct)
        {
            if (req is null)
                return BadRequest(new { message = "Request inválido." });

            if (req.CfdiId == Guid.Empty)
                return BadRequest(new { message = "CfdiId es requerido." });

            if (req.Conceptos is null || req.Conceptos.Count == 0)
                return BadRequest(new { message = "Debes enviar al menos un concepto." });

            if (req.Conceptos.Any(x => x.CfdiConceptoId == Guid.Empty))
                return BadRequest(new { message = "CfdiConceptoId inválido." });

            if (req.Conceptos.Any(x => x.Cantidad <= 0))
                return BadRequest(new { message = "Cantidad debe ser mayor a 0." });

            var cuentaId = Guid.Parse(User.GetCuentaId());

            var dto = await _facturacionService.CrearNotaCreditoDevolucionAsync(
                cuentaId,
                req.CfdiId,
                req.Conceptos,
                ct);

            return Ok(dto);
        }
    }
}
