using Facturacion.API.Models.Dto.Cliente.Catalogos;

namespace Facturacion.API.Services.Interface
{
    public interface ICatalogoService
    {
        Task<List<GetConceptosDto>> BuscarConceptosAsync(string search, int take = 20);
        Task<List<GetClaveUnidadDto>> BuscarClavesUnidadAsync(string search, int take = 20);
    }
}
