using Facturacion.API.Models.Dto.Cliente.Sucursal;

namespace Facturacion.API.Services.Interface
{
    public interface ISucursalService
    {
        Task<Guid> CreateAsync(SucursalCreateDto dto, Guid cuentaId, string usuarioId, CancellationToken ct);

        Task<PagedResult<SucursalRowDto>> GetSucursalesAsync(
        Guid cuentaId,
        string usuarioId,
        SucursalesQueryDto query,
        CancellationToken ct
    );

        Task<ToggleActivoResponse_Sucursal> SetActivoAsync(Guid cuentaId, string usuarioId, Guid sucursalId, bool activo, CancellationToken ct);
        Task<SucursalDetalleDto> GetSucursalByIdAsync(Guid cuentaId, Guid sucursalId, CancellationToken ct);


        Task<Guid> CreateSucursalAsync(Guid cuentaId, string usuarioId, SucursalCreateDto dto, CancellationToken ct);
        Task UpdateSucursalAsync(Guid cuentaId, string usuarioId, Guid sucursalId, SucursalUpdateDto dto, CancellationToken ct);

        Task<SerieFolioPreviewDto> GetSerieFolioPreviewAsync(Guid cuentaId, Guid sucursalId, string conceptoSerie, CancellationToken ct);
    }
}
