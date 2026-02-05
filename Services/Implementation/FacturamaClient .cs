using Facturacion.API.Models.Dto.Cliente.Factura;
using Facturacion.API.Services.Interface;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Facturacion.API.Services.Implementation
{
    public sealed class FacturamaClient : IFacturamaClient
    {
        private readonly HttpClient _http;

        public FacturamaClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<JsonDocument> CrearCfdiMultiAsync(FacturamaCfdiRequest payload, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null // Respeta nombres tal cual en clases (PaymentForm, CfdiType, etc.)
            });

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var resp = await _http.PostAsync("api-lite/3/cfdis", content, ct); // baseAddress ya configurado

            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                // regresa el error tal cual para debug (puedes mapearlo bonito)
                throw new InvalidOperationException($"Facturama error: {(int)resp.StatusCode} - {body}");
            }

            return JsonDocument.Parse(body);
        }
        //--------------------CSDS---------------------
        public async Task<bool> ExisteCsdAsync(string rfc, CancellationToken ct = default)
        {
            using var resp = await _http.GetAsync($"api-lite/csds/{rfc}", ct); // GET por RFC :contentReference[oaicite:6]{index=6}
            if (resp.StatusCode == HttpStatusCode.NotFound) return false;
            return resp.IsSuccessStatusCode;
        }

        public async Task CrearCsdAsync(FacturamaCsdRequest payload, CancellationToken ct = default)
        {
            // POST api-lite/csds :contentReference[oaicite:7]{index=7}
            await SendAsync(HttpMethod.Post, "api-lite/csds", payload, ct);
        }

        public async Task ActualizarCsdAsync(string rfc, FacturamaCsdRequest payload, CancellationToken ct = default)
        {
            // PUT api-lite/csds/{rfc} :contentReference[oaicite:8]{index=8}
            await SendAsync(HttpMethod.Put, $"api-lite/csds/{rfc}", payload, ct);
        }

        private async Task SendAsync(HttpMethod method, string url, FacturamaCsdRequest payload, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = null });
            using var msg = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using var resp = await _http.SendAsync(msg, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"error: {(int)resp.StatusCode} - {body}");
        }

        public async Task<(JsonDocument Doc, string Body)> CrearCfdiMultiRawAsync(FacturamaCfdiRequest payload, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });

            try
            {
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var resp = await _http.PostAsync("api-lite/3/cfdis", content, ct);

                var body = await resp.Content.ReadAsStringAsync(ct);

                if (!resp.IsSuccessStatusCode)
                    throw new InvalidOperationException($"error: {(int)resp.StatusCode} - {body}");

                return (JsonDocument.Parse(body), body);
            }
            catch (Exception ex) {
                throw;
            }
           
        }

        // Facturama: GET api/Cfdi/{format}/{type}/{id} -> FileViewModel (base64)
        public async Task<FacturamaFileViewModel> DownloadCfdiAsync(string id, string format, string type, CancellationToken ct)
        {
            // Ej: api/Cfdi/pdf/issued/{id}
            var url = $"api/Cfdi/{format}/{type}/{id}";
            var resp = await _http.GetAsync(url, ct);
            resp.EnsureSuccessStatusCode();

            var vm = await resp.Content.ReadFromJsonAsync<FacturamaFileViewModel>(cancellationToken: ct);
            if (vm is null || string.IsNullOrWhiteSpace(vm.Content))
                throw new InvalidOperationException("Facturama devolvió respuesta vacía al descargar el archivo.");

            return vm;
        }

        // Facturama: GET cfdi/zip?id={id}&type={type}
        // OJO: algunos entornos responden JSON; otros responden directamente binario.
        public async Task<byte[]> DownloadZipAsync(string id, string type, CancellationToken ct)
        {
            var url = $"cfdi/zip?id={Uri.EscapeDataString(id)}&type={Uri.EscapeDataString(type)}";
            var resp = await _http.GetAsync(url, ct);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsByteArrayAsync(ct);
        }

        public async Task<CancelCfdiResultDto> CancelCfdiAsync(string facturamaId, string type, string motive, Guid? uuidReplacement, CancellationToken ct)
        {
            // /api-lite/cfdis/{id}?motive=..&uuidReplacement=..
            var url = $"api-lite/cfdis/{Uri.EscapeDataString(facturamaId)}?motive={Uri.EscapeDataString(motive)}";

            if (uuidReplacement is not null)
                url += $"&uuidReplacement={uuidReplacement.Value:D}";

            using var req = new HttpRequestMessage(HttpMethod.Delete, url);
            using var resp = await _http.SendAsync(req, ct);
            resp.EnsureSuccessStatusCode();

            var result = await resp.Content.ReadFromJsonAsync<CancelCfdiResultDto>(cancellationToken: ct);
            if (result is null) throw new InvalidOperationException("Respuesta vacía de Facturama al cancelar CFDI.");

            return result;
        }
    }
}
