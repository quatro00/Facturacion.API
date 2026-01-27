using Facturacion.API.Models.Dto.Cliente.Perfil;

namespace Facturacion.API.Services.Interface
{
    public interface IPerfilService
    {
        Task GuardarSellosAsync(Guid cuentaId, EnviarSellosDigitalesRequest request);
        Task SubirSellosAFacturamaAsync(Guid cuentaId, bool force = false, CancellationToken ct = default);
    }
}
