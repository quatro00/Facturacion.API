using Facturacion.API.Models.Dto.Cliente.Cliente;

namespace Facturacion.API.Services.Interface
{
    public interface IClienteService
    {
        Task<Guid> CrearClienteAsync(CrearClienteRequest request, string userId, Guid cuentaId);
        Task<Guid> ActualizarCliente(ActualizarClienteDto request, string userId, Guid clienteId);
        Task<List<ClienteListadoDto>> ObtenerClientesAsync(Guid cuentaId);
        Task<ClienteDetalleDto?> GetByIdAsync(Guid id);

    }
}
