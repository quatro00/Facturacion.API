using Facturacion.API.Models.Dto.Cliente.Factura;
using System.Text.Json;

namespace Facturacion.API.Services.Interface
{
    public interface IFacturacionService
    {
        Task<JsonDocument> EmitirCfdiMultiAsync(EmitirCfdiRequest req, Guid cuentaId, CancellationToken ct = default);
    }
}
