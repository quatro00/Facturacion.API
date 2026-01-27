using Facturacion.API.Models.Dto.Cliente.Factura;
using System.Text.Json;

namespace Facturacion.API.Services.Interface
{
    public interface IFacturamaClient
    {
        Task<JsonDocument> CrearCfdiMultiAsync(FacturamaCfdiRequest payload, CancellationToken ct = default);
        Task<bool> ExisteCsdAsync(string rfc, CancellationToken ct = default);
        Task CrearCsdAsync(FacturamaCsdRequest payload, CancellationToken ct = default);
        Task ActualizarCsdAsync(string rfc, FacturamaCsdRequest payload, CancellationToken ct = default);
        Task<(JsonDocument Doc, string Body)> CrearCfdiMultiRawAsync(FacturamaCfdiRequest payload, CancellationToken ct = default);
    }
}
