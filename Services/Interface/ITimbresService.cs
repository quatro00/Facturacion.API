namespace Facturacion.API.Services.Interface
{
    public interface ITimbresService
    {
        Task AsegurarSaldoCuentaAsync(Guid cuentaId, CancellationToken ct);
        Task ValidarDisponiblesAsync(Guid cuentaId, CancellationToken ct); // bloquea
        Task RegistrarConsumoTimbradoAsync(Guid cuentaId, Guid? cfdiId, string? facturamaId, string? uuid, string accion, string? createdBy, CancellationToken ct);
        Task RegistrarCompraOAjusteAsync(Guid cuentaId, int timbres, string accion, string? referencia, string? notas, string? createdBy, CancellationToken ct);
        Task<(int disponibles, int consumidos)> GetResumenAsync(Guid cuentaId, CancellationToken ct);
    }
}
