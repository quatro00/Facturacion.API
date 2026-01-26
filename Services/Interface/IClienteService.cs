using Facturacion.API.Models.Dto.Cliente.Cliente;

namespace Facturacion.API.Services.Interface
{
    public interface IClienteService
    {
        Task<Guid> CrearClienteAsync(CrearClienteRequest request, string userId, Guid cuentaId);
        Task<List<ClienteListadoDto>> ObtenerClientesAsync(Guid cuentaId);
    }
}
