using Facturacion.API.Models.Dto.Cliente.Factura;
using System.Text.Json;

namespace Facturacion.API.Services.Interface
{
    public interface IFacturamaClient
    {
        Task<JsonDocument> CrearCfdiMultiAsync(FacturamaCfdiRequest payload, CancellationToken ct = default);
        Task<CancelCfdiResultDto> CancelCfdiAsync(string facturamaId, string type, string motive, Guid? uuidReplacement, CancellationToken ct);
        Task<bool> ExisteCsdAsync(string rfc, CancellationToken ct = default);
        Task CrearCsdAsync(FacturamaCsdRequest payload, CancellationToken ct = default);
        Task ActualizarCsdAsync(string rfc, FacturamaCsdRequest payload, CancellationToken ct = default);
        Task<(JsonDocument Doc, string Body)> CrearCfdiMultiRawAsync(FacturamaCfdiRequest payload, CancellationToken ct = default);
        Task<FacturamaFileViewModel> DownloadCfdiAsync(string id, string format, string type, CancellationToken ct);
        Task<byte[]> DownloadZipAsync(string id, string type, CancellationToken ct);
    }
}
