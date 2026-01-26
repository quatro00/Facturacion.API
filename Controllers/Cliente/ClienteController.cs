using Facturacion.API.Helpers;
using Facturacion.API.Models.Dto.Cliente.Cliente;
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

        public ClienteController(IClienteService clienteService)
        {
            _clienteService = clienteService;
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
        public IActionResult ObtenerCliente(Guid id)
        {
            return Ok(); // ejemplo
        }
    }
}
