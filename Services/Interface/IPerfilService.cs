using Facturacion.API.Models.Dto.Cliente.Perfil;
using Facturacion.API.Models.Perfil;

namespace Facturacion.API.Services.Interface
{
    public interface IPerfilService
    {
        // Razones sociales
        Task<IReadOnlyList<RazonSocialDto>> GetRazonesSocialesAsync(Guid cuentaId, CancellationToken ct = default);
        Task<Guid> UpsertRazonSocialAsync(Guid cuentaId, string usuarioId, UpsertRazonSocialRequest dto, CancellationToken ct = default);
        Task DeleteRazonSocialAsync(Guid cuentaId, Guid razonSocialId, CancellationToken ct = default);
        Task SetDefaultRazonSocialAsync(Guid cuentaId, Guid razonSocialId, CancellationToken ct = default);

        // ✅ Sellos por Razón Social (Emisor)
        Task GuardarSellosAsync(Guid cuentaId, Guid razonSocialId, EnviarSellosDigitalesRequest request, CancellationToken ct = default);
        Task SubirSellosAFacturamaAsync(Guid cuentaId, Guid razonSocialId, bool force = false, CancellationToken ct = default);
        Task<Stream> DescargarSellosAsync(Guid cuentaId, Guid razonSocialId, CancellationToken ct = default);
    }
}
