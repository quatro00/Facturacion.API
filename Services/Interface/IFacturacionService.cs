using Facturacion.API.Models.Dto;
using Facturacion.API.Models.Dto.Cliente.Factura;
using System.Text.Json;

namespace Facturacion.API.Services.Interface
{
    public interface IFacturacionService
    {
        Task<JsonDocument> EmitirCfdiMultiAsync(EmitirCfdiRequest req, Guid cuentaId, CancellationToken ct = default);
        Task<CancelCfdiResultDto> CancelarCfdiAsync(Guid cfdiId, Guid cuentaId, CancelCfdiRequestDto req, CancellationToken ct = default);
        Task<PagedResult<FacturaListItemDto>> GetFacturasAsync(Guid cuentaId, GetFacturasQuery query, CancellationToken ct);
        Task<(byte[] bytes, string filename, string contentType)> GetAcuseCancelacionAsync(Guid cfdiId, Guid cuentaId, CancellationToken ct);
        Task<(byte[] bytes, string filename, string contentType)> GetXmlAsync(string id, string type, CancellationToken ct);
        Task<(byte[] bytes, string filename, string contentType)> GetPdfAsync(string id, string type, CancellationToken ct);
        Task<(byte[] bytes, string filename, string contentType)> GetZipAsync(string id, string type, CancellationToken ct);
        Task<ReenviarCfdiResponseDto> ReenviarCfdiAsync(Guid cfdiId, Guid cuentaId, ReenviarCfdiRequestDto req, CancellationToken ct = default);
        Task<CfdiDetalleDto> GetCfdiDetalleAsync(Guid cfdiId, Guid cuentaId, CancellationToken ct);
        Task<CfdiCreadoDto> CrearNotaCreditoTotalAsync(Guid cuentaId, Guid RazonSocialId, Guid cfdiId, CancellationToken ct);
    }
}
