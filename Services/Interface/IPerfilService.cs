using Facturacion.API.Models.Dto.Cliente.Perfil;

namespace Facturacion.API.Services.Interface
{
    public interface IPerfilService
    {
        Task GuardarSellosAsync(Guid cuentaId, EnviarSellosDigitalesRequest request);
    }
}
