using Facturacion.API.Helpers;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto.Cliente.Cliente;
using Facturacion.API.Repositories.Interface;
using Facturacion.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.API.Controllers.Cliente
{
    [Route("api/cliente/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly IGenericRepository<Facturacion.API.Models.Domain.Cliente> _repo;

        public ClienteController(
            IClienteService clienteService,
            IGenericRepository<Facturacion.API.Models.Domain.Cliente> _repo
            )
        {
            _clienteService = clienteService;
            this._repo = _repo;
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost]
        public async Task<IActionResult> CrearCliente([FromBody] CrearClienteRequest request)
        {
            if (request.MetodoPago == "") { request.MetodoPago = null; }
            if (request.FormaPago == "") { request.FormaPago = null; }
            if (request.Moneda == "") { request.Moneda = null; }
            if (request.Exportacion == "") { request.Exportacion = null; }
            if (request.UsoCfdi == "") { request.UsoCfdi = null; }

            var clienteId = await _clienteService.CrearClienteAsync(request, User.GetId(), Guid.Parse(User.GetCuentaId()));

            return CreatedAtAction(
                nameof(ObtenerCliente),
                new { id = clienteId },
                new { clienteId }
            );
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet]
        public async Task<IActionResult> ObtenerClientes()
        {

            var clientes = await _clienteService.ObtenerClientesAsync(Guid.Parse(User.GetCuentaId()));
            return Ok(clientes);
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerCliente(Guid id)
        {
            var cliente = await _clienteService.GetByIdAsync(id);

            if (cliente is null)
                return NotFound();

            return Ok(cliente);
        }

        [Authorize(Roles = "Cliente")]
        [HttpPatch("{id}/UpdateActivo")]
        public async Task<IActionResult> UpdateActivo(Guid id, [FromBody] UpdateClienteActivoDto dto)
        {
            var cliente = await this._repo.GetByIdAsync(id);
            if (cliente is null)
                return NotFound(new { message = "Cliente no encontrado" });

            //if (cliente.Activo == dto.Activo)
            //    return Ok(new { id = cliente.Id, activo = cliente.Activo }); // idempotente

            cliente.Activo = dto.Activo;
            cliente.UsuarioModificacion = User.GetId();
            cliente.FechaModificacion = DateTime.UtcNow;

            await this._repo.UpdateAsync(cliente);

            return Ok(new { id = cliente.Id, activo = cliente.Activo });
        }

        [Authorize(Roles = "Cliente")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateCliente(Guid id, [FromBody] ActualizarClienteDto request)
        {
            if (request.MetodoPago == "") { request.MetodoPago = null; }
            if (request.FormaPago == "") { request.FormaPago = null; }
            if (request.Moneda == "") { request.Moneda = null; }
            if (request.Exportacion == "") { request.Exportacion = null; }
            if (request.UsoCfdi == "") { request.UsoCfdi = null; }

            var clienteId = await _clienteService.ActualizarCliente(request, User.GetId(), id);

            return CreatedAtAction(
                nameof(ObtenerCliente),
                new { id = clienteId },
                new { clienteId }
            );
        }
    }
}
