using Facturacion.API.Helpers;
using Facturacion.API.Models.Dto.Cliente.Sucursal;
using Facturacion.API.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.API.Controllers.Cliente
{
    [Route("api/Cliente/[controller]")]
    [ApiController]
    public class SucursalController : ControllerBase
    {
        private readonly ISucursalService _service;

        public SucursalController(ISucursalService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] SucursalCreateDto dto, CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());
            var usuarioId = User.GetId();

            var id = await _service.CreateSucursalAsync(cuentaId, usuarioId, dto, ct);
            return Ok(new { id });
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<SucursalRowDto>>> Get([FromQuery] SucursalesQueryDto query, CancellationToken ct)
        {
            var cuentaId = User.GetCuentaId();
            var usuarioId = User.GetId();

            var result = await _service.GetSucursalesAsync(Guid.Parse(cuentaId), usuarioId, query, ct);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/activo")]
        public async Task<ActionResult<ToggleActivoResponse_Sucursal>> SetActivo(
        [FromRoute] Guid id,
        [FromBody] ToggleActivoResponse_Sucursal req,
        CancellationToken ct)
        {
            var cuentaId = User.GetCuentaId();
            var usuarioId = User.GetId();

            var result = await _service.SetActivoAsync(Guid.Parse(cuentaId), usuarioId, id, req.Activo, ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SucursalDetalleDto>> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());
            var dto = await _service.GetSucursalByIdAsync(cuentaId, id, ct);
            return Ok(dto);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Update([FromRoute] Guid id, [FromBody] SucursalUpdateDto dto, CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());
            var usuarioId = User.GetId();

            await _service.UpdateSucursalAsync(cuentaId, usuarioId, id, dto, ct);
            return NoContent();
        }

        [HttpGet("{sucursalId:guid}/serie-folio")]
        public async Task<ActionResult<SerieFolioPreviewDto>> GetSerieFolioPreview(
        Guid sucursalId,
        [FromQuery] string conceptoSerie,
        CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());

            var dto = await _service.GetSerieFolioPreviewAsync(cuentaId, sucursalId, conceptoSerie, ct);
            return Ok(dto);
        }
    }
}
