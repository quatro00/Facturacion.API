using Facturacion.API.Data;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto;
using Facturacion.API.Models.Dto.Cliente.Factura;
using Facturacion.API.Models.Dto.Constants;
using Facturacion.API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace Facturacion.API.Services.Implementation
{
    public class FacturacionService : IFacturacionService
    {
        private readonly FacturacionContext _context;
        private readonly IFacturamaClient _facturama;
        private readonly IEmailSender _emailSender;

        public FacturacionService(FacturacionContext context, IFacturamaClient facturama, IEmailSender emailSender)
        {
            _context = context;
            _facturama = facturama;
            _emailSender = emailSender;
        }
        //----helpers----
        private static decimal R2(decimal v) =>
    Math.Round(v, 2, MidpointRounding.AwayFromZero);

        private static string GetString(JsonElement e, string prop)
            => e.TryGetProperty(prop, out var p) ? p.GetString() ?? "" : "";

        private static decimal GetDecimal(JsonElement e, string prop)
        {
            if (!e.TryGetProperty(prop, out var p)) return 0m;
            if (p.ValueKind == JsonValueKind.Number) return p.GetDecimal();
            if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out var d)) return d;
            return 0m;
        }

        public async Task<JsonDocument> EmitirCfdiMultiAsync(EmitirCfdiRequest req, Guid cuentaId, CancellationToken ct = default)
        {
            // 1) Cliente
            var cliente = await _context.Clientes
                .Include(x => x.RegimenFiscal)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == req.ClienteId /* && x.CuentaId == cuentaId */, ct);

            if (cliente is null)
                throw new KeyNotFoundException("Cliente no encontrado");

            // 2) Config cliente
            var config = await _context.ClienteConfiguracions.AsNoTracking()
                .Where(x => x.ClienteId == req.ClienteId)
                .OrderByDescending(x => x.FechaCreacion)
                .FirstOrDefaultAsync(ct);

            // 3) Emisor por cuenta
            var emisor = await _context.RazonSocials
                .Include(x => x.RegimenFiscal)
                .FirstOrDefaultAsync(x => x.CuentaId == cuentaId, ct);

            if (emisor is null)
                throw new InvalidOperationException("No hay emisor configurado");

            req.ExpeditionPlace = emisor.CodigoPostal;

            // 4) Armar payload Facturama
            var payload = new FacturamaCfdiRequest
            {
                Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                Serie = req.Serie,
                Folio = req.Folio,
                CfdiType = string.IsNullOrWhiteSpace(req.CfdiType) ? "I" : req.CfdiType,
                Currency = req.Currency,
                ExpeditionPlace = req.ExpeditionPlace,
                Exportation = req.Exportation,

                PaymentForm = req.PaymentForm ?? config?.FormaPago,
                PaymentMethod = req.PaymentMethod ?? config?.MetodoPago,

                Issuer = new FacturamaIssuer
                {
                    Rfc = emisor.Rfc,
                    Name = emisor.RazonSocial1,
                    FiscalRegime = emisor.RegimenFiscal.RegimenFiscal
                },
                Receiver = new FacturamaReceiver
                {
                    Rfc = cliente.Rfc,
                    Name = cliente.RazonSocial,
                    CfdiUse = req.CfdiUse ?? config?.UsoCfdiDefault ?? "G03",
                    FiscalRegime = cliente.RegimenFiscal.RegimenFiscal,
                    TaxZipCode = cliente.CodigoPostal
                },
                Items = req.Items.Select(i => new FacturamaItem
                {
                    ProductCode = i.ProductCode,
                    UnitCode = i.UnitCode,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TaxObject = string.IsNullOrWhiteSpace(i.TaxObject) ? "02" : i.TaxObject,
                    Taxes = (i.Taxes ?? new()).Select(tx => new FacturamaTax
                    {
                        Name = tx.Name,
                        Rate = tx.Rate,
                        Base = tx.Base,
                        Total = tx.Total,
                        IsRetention = tx.IsRetention
                    }).ToList()
                }).ToList()
            };

            // 4.1 Normaliza Subtotal/Taxes/Total por item (tu bloque, mejorado)
            foreach (var item in payload.Items)
            {
                item.Subtotal = R2(item.Quantity * item.UnitPrice);

                if (item.TaxObject == "01")
                {
                    item.Taxes?.Clear();
                    item.Total = item.Subtotal;
                    continue;
                }

                if (item.Taxes != null && item.Taxes.Count > 0)
                {
                    foreach (var tx in item.Taxes)
                    {
                        tx.Base = R2(item.Subtotal);
                        tx.Total = R2(tx.Base * tx.Rate);
                    }
                }

                var traslados = item.Taxes?.Where(t => !t.IsRetention).Sum(t => t.Total) ?? 0m;
                var retenciones = item.Taxes?.Where(t => t.IsRetention).Sum(t => t.Total) ?? 0m;
                item.Total = R2(item.Subtotal + traslados - retenciones);
            }

            // 4.2 Serializa EXACTAMENTE lo que mandas (para Raw)
            var requestJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = null });

            // 5) Llamar a Facturama, pero ahora devuelve body string para guardarlo tal cual
            var (doc, responseBody) = await _facturama.CrearCfdiMultiRawAsync(payload, ct);

            // 6) Guardar en BD (Cfdi + Detalle + Raw + Historial) dentro de una transacción
            await GuardarCfdiAsync(cuentaId, req, payload, requestJson, responseBody, doc, ct);

            return doc;
        }

        private static int MapFacturamaStatusToCfdiStatusId(string? facturamaStatus)
        {
            var s = (facturamaStatus ?? "").Trim().ToLowerInvariant();

            return s switch
            {
                "active" or "valid" or "issued" => CfdiStatusIds.TIMBRADO,
                "canceled" or "cancelled" => CfdiStatusIds.CANCELADO,
                "requested" => CfdiStatusIds.CANCELACION_SOLICITADA,
                "rejected" => CfdiStatusIds.CANCELACION_RECHAZADA,
                _ => CfdiStatusIds.ERROR_TIMBRADO
            };
        }

        private static string? GetStringByPath(JsonElement root, params string[] path)
        {
            JsonElement current = root;

            foreach (var key in path)
            {
                if (current.ValueKind != JsonValueKind.Object)
                    return null;

                if (!current.TryGetProperty(key, out var next))
                    return null;

                current = next;
            }

            return current.ValueKind == JsonValueKind.String
                ? current.GetString()
                : current.GetRawText();
        }

        private async Task<Guid> GuardarCfdiAsync(
    Guid cuentaId,
    EmitirCfdiRequest req,
    FacturamaCfdiRequest payload,
    string requestJson,
    string responseJson,
    JsonDocument facturamaDoc,
    CancellationToken ct,
    Guid? cfdiOrigenId = null,
    string? tipoRelacionSat = null
)
        {
            var root = facturamaDoc.RootElement;

            // Extrae UUID timbrado (viene en Complement.TaxStamp.Uuid)
            var uuidStr = root
                .GetProperty("Complement")
                .GetProperty("TaxStamp")
                .GetProperty("Uuid")
                .GetString();

            if (string.IsNullOrWhiteSpace(uuidStr))
                throw new InvalidOperationException("No se encontró UUID en respuesta del PAC.");

            var uuid = Guid.Parse(uuidStr);

            // Totales del CFDI (nivel comprobante)
            var subtotal = GetDecimal(root, "Subtotal");
            var total = GetDecimal(root, "Total");

            // Serie / Folio (puede venir como string)
            var serie = GetString(root, "Serie");
            var folio = GetString(root, "Folio");

            // Fecha
            var fechaStr = GetString(root, "Date");
            var fechaTimbrado = DateTime.TryParse(fechaStr, out var dt) ? dt : DateTime.UtcNow;

            // RFCs
            var rfcEmisor = root.GetProperty("Issuer").GetProperty("Rfc").GetString() ?? "";
            var razonSocialEmisor =
    GetStringByPath(root, "Issuer", "TaxName")
    ?? GetStringByPath(root, "Issuer", "Name");


            var rfcReceptor = root.GetProperty("Receiver").GetProperty("Rfc").GetString() ?? "";
            var razonSocialReceptor =
    GetStringByPath(root, "Receiver", "Name")
    ?? GetStringByPath(root, "Receiver", "TaxName");

            // Status
            var facturamaStatus = GetString(root, "Status");
            var cfdiStatusId = MapFacturamaStatusToCfdiStatusId(facturamaStatus);


            string? emisorRegimen = payload.Issuer?.FiscalRegime
    ?? GetStringByPath(root, "Issuer", "FiscalRegime");

            string? receptorRegimen = payload.Receiver?.FiscalRegime
                ?? GetStringByPath(root, "Receiver", "FiscalRegime");

            string? receptorCp = payload.Receiver?.TaxZipCode
                ?? GetStringByPath(root, "Receiver", "TaxZipCode");

            string? usoCfdi = payload.Receiver?.CfdiUse
                ?? GetStringByPath(root, "Receiver", "CfdiUse");

            string? exportacion = payload.Exportation
                ?? GetString(root, "Exportation");

            var status = GetString(root, "Status");
            //var estatus = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase) ? "Activo" : status;
            var estatusClave = await _context.CfdiStatuses
            .Where(x => x.Id == cfdiStatusId)
            .Select(x => x.Clave)
            .FirstAsync(ct);

            var facturamaId = root.GetProperty("Id").GetString();

            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                // 1) Cfdi (principal)
                // 1) Cfdi (principal)
                var cfdi = new Cfdi
                {
                    Id = Guid.NewGuid(),
                    CuentaId = cuentaId,
                    ClienteId = req.ClienteId,
                    FacturamaId = facturamaId ?? "",

                    Uuid = uuid,
                    Serie = string.IsNullOrWhiteSpace(serie) ? req.Serie : serie,
                    Folio = string.IsNullOrWhiteSpace(folio) ? req.Folio : folio,

                    TipoCfdi = string.IsNullOrWhiteSpace(req.CfdiType) ? "I" : req.CfdiType,
                    FechaTimbrado = fechaTimbrado,

                    Subtotal = subtotal,
                    Descuento = 0m,
                    Total = total,

                    Moneda = req.Currency ?? "MXN",
                    FormaPago = req.PaymentForm,
                    MetodoPago = req.PaymentMethod,
                    LugarExpedicion = req.ExpeditionPlace,

                    RfcEmisor = rfcEmisor,
                    RazonSocialEmisor = razonSocialEmisor,

                    RfcReceptor = rfcReceptor,
                    RazonSocialReceptor = razonSocialReceptor,

                    Pac = "Facturama",
                    Estatus = estatusClave,
                    CfdiStatusId = cfdiStatusId,

                    CreatedAt = DateTime.UtcNow,

                    // ✅ NUEVOS CAMPOS (snapshot fiscal desde payload)
                    EmisorRegimenFiscal = emisorRegimen,       // "605"
                    ReceptorRegimenFiscal = receptorRegimen,   // "612"
                    ReceptorTaxZipCode = receptorCp,        // "01160"
                    UsoCfdi = usoCfdi,                      // "G03"
                    Exportacion = exportacion,                         // "01"
                    CfdiOrigenId = cfdiOrigenId,
                    TipoRelacionSat = tipoRelacionSat,
                };

                _context.Cfdis.Add(cfdi);
                await _context.SaveChangesAsync(ct);

                // 2) Conceptos + Impuestos (desde payload, porque trae TaxObject/Taxes)
                foreach (var it in payload.Items)
                {
                    var concepto = new CfdiConcepto
                    {
                        Id = Guid.NewGuid(),
                        CuentaId = cuentaId,
                        CfdiId = cfdi.Id,

                        ClaveProdServ = it.ProductCode,
                        Descripcion = it.Description,
                        Cantidad = it.Quantity,
                        ValorUnitario = it.UnitPrice,          // o it.UnitValue si así se llama en tu request

                        Descuento = it.Discount,               // ✅ NUEVO (si existe)
                        ClaveUnidad = it.UnitCode,             // ✅ ya lo tienes
                        Unidad = it.Unit,                      // ✅ NUEVO (si existe)
                        NoIdentificacion = it.IdentificationNumber, // ✅ NUEVO (si existe)

                        // Si tu Subtotal en request ya trae el importe antes de impuestos, úsalo:
                        Subtotal = it.Subtotal,

                        // Total de concepto normalmente es el importe (sin impuestos) en muchos diseños,
                        // pero tú ya lo manejas, así que respétalo:
                        Total = it.Total,

                        ObjetoImp = it.TaxObject,
                        CreatedAt = DateTime.UtcNow,
                    };

                    _context.CfdiConceptos.Add(concepto);

                    if (it.Taxes != null)
                    {
                        foreach (var tax in it.Taxes)
                        {
                            var imp = new CfdiConceptoImpuesto
                            {
                                Id = Guid.NewGuid(),
                                CuentaId = cuentaId,
                                CfdiConceptoId = concepto.Id,

                                TipoImpuesto = tax.Name,
                                ImpuestoClave = MapImpuestoClave(tax.Name), // ✅ si existe en tabla
                                TipoFactor = "Tasa",                        // ✅ si existe en tabla (o tax.TypeFactor)

                                Tasa = tax.Rate,
                                Base = tax.Base,
                                Importe = tax.Total,
                                EsRetencion = tax.IsRetention,

                                CreatedAt = DateTime.UtcNow
                            };

                            _context.CfdiConceptoImpuestos.Add(imp);
                        }
                    }
                }

                await _context.SaveChangesAsync(ct);

                // 3) Raw
                var raw = new CfdiRaw
                {
                    Id = Guid.NewGuid(),
                    CuentaId = cuentaId,
                    CfdiId = cfdi.Id,

                    RequestJson = requestJson,
                    ResponseJson = responseJson,

                    XmlPath = null,
                    PdfPath = null,

                    CreatedAt = DateTime.UtcNow
                };

                _context.CfdiRaws.Add(raw);

                // 4) Historial inicial
                var hist = new CfdiEstatusHistorial
                {
                    Id = Guid.NewGuid(),
                    CuentaId = cuentaId,
                    CfdiId = cfdi.Id,
                    Estatus = estatusClave,
                    Motivo = null,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CfdiEstatusHistorials.Add(hist);

                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return cfdi.Id;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        static string? MapImpuestoClave(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return name.Trim().ToUpperInvariant() switch
            {
                "IVA" => "002",
                "ISR" => "001",
                "IEPS" => "003",
                _ => null
            };
        }
        public async Task<PagedResult<FacturaListItemDto>> GetFacturasAsync(
       Guid cuentaId,
       GetFacturasQuery q,
       CancellationToken ct)
        {
            var query = _context.Cfdis
                .Include(x => x.CfdiOrigen)
                .AsNoTracking()
                .Where(x => x.CuentaId == cuentaId);

            // 📅 Fechas
            if (q.From.HasValue)
                query = query.Where(x => x.FechaTimbrado >= q.From.Value.Date);

            if (q.To.HasValue)
                query = query.Where(x => x.FechaTimbrado <= q.To.Value.Date.AddDays(1).AddTicks(-1));

            // 🟢 Estatus
            if (!string.IsNullOrWhiteSpace(q.Status))
                query = query.Where(x => x.Estatus == q.Status);

            // 🧾 Tipo
            if (!string.IsNullOrWhiteSpace(q.Type))
                query = query.Where(x => x.TipoCfdi == q.Type);

            // 💱 Moneda
            if (!string.IsNullOrWhiteSpace(q.Currency))
                query = query.Where(x => x.Moneda == q.Currency);

            // 🔍 Búsqueda libre
            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim();

                query = query.Where(x =>
                    x.Uuid.ToString().Contains(s) ||
                    x.RfcReceptor.Contains(s) ||
                    (x.Serie + "-" + x.Folio).Contains(s)
                );
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.FechaTimbrado)
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(x => new FacturaListItemDto
                {
                    Id = x.Id,
                    FacturamaId = x.FacturamaId,
                    Uuid = x.Uuid.ToString(),

                    Fecha = x.FechaTimbrado,
                    Serie = x.Serie,
                    Folio = x.Folio,

                    ReceptorRfc = x.RfcReceptor,
                    ReceptorNombre = x.RazonSocialReceptor ?? "",

                    Tipo = x.TipoCfdi,
                    TipoNombre = x.TipoCfdi == "E" ? "Nota de crédito" : "Factura",

                    Moneda = x.Moneda,
                    Total = x.Total,
                    Estatus = x.Estatus,

                    // ✅ Relación NC -> Origen
                    CfdiOrigenId = x.CfdiOrigenId,
                    OrigenUuid = x.CfdiOrigen != null ? x.CfdiOrigen.Uuid.ToString() : null,
                    OrigenSerie = x.CfdiOrigen != null ? x.CfdiOrigen.Serie : null,
                    OrigenFolio = x.CfdiOrigen != null ? x.CfdiOrigen.Folio : null,
                })
                .ToListAsync(ct);

            return new PagedResult<FacturaListItemDto>
            {
                Items = items,
                Total = total
            };
        }

        public async Task<(byte[] bytes, string filename, string contentType)> GetXmlAsync(string id, string type, CancellationToken ct)
        => await DownloadBase64Async(id, "xml", type, "application/xml", ".xml", ct);

        public async Task<(byte[] bytes, string filename, string contentType)> GetPdfAsync(string id, string type, CancellationToken ct)
            => await DownloadBase64Async(id, "pdf", type, "application/pdf", ".pdf", ct);

        public async Task<(byte[] bytes, string filename, string contentType)> GetZipAsync(string id, string type, CancellationToken ct)
        {
            var bytes = await _facturama.DownloadZipAsync(id, type, ct);
            var filename = $"CFDI_{id}.zip";
            return (bytes, filename, "application/zip");
        }

        private async Task<(byte[] bytes, string filename, string contentType)> DownloadBase64Async(
        string id, string format, string type, string mime, string ext, CancellationToken ct)
        {
            var vm = await _facturama.DownloadCfdiAsync(id, format, type, ct);

            // vm.Content es base64
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(vm.Content!);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("El contenido recibido no es Base64 válido.");
            }

            var filename = $"CFDI_{id}{ext}";
            return (bytes, filename, mime);
        }

        public async Task<CancelCfdiResultDto> CancelarCfdiAsync(
    Guid cfdiId,
    Guid cuentaId,
    CancelCfdiRequestDto req,
    CancellationToken ct = default)
        {
            if (req is null)
                throw new ArgumentNullException(nameof(req));

            // 1) Validaciones SAT
            var motive = (req.Motive ?? "02").Trim();
            if (motive is not ("01" or "02" or "03" or "04"))
                throw new InvalidOperationException("Motivo inválido. Usa 01, 02, 03 o 04.");

            if (motive == "01" && req.UuidReplacement is null)
                throw new InvalidOperationException("UuidReplacement es requerido cuando Motive = 01.");

            // 2) Traer CFDI (y status actual si tienes navegación)
            var cfdi = await _context.Cfdis
                .FirstOrDefaultAsync(x => x.Id == cfdiId && x.CuentaId == cuentaId, ct);

            if (cfdi is null)
                throw new KeyNotFoundException("CFDI no encontrado para la cuenta.");

            if (string.IsNullOrWhiteSpace(cfdi.FacturamaId))
                throw new InvalidOperationException("El CFDI no tiene FacturamaId.");

            // 3) Obtener status actual desde catálogo para reglas
            var statusActual = await _context.CfdiStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == cfdi.CfdiStatusId, ct);

            if (statusActual is null)
                throw new InvalidOperationException("El CFDI tiene un CfdiStatusId inválido.");

            if (statusActual.EsFinal)
                throw new InvalidOperationException($"El CFDI no se puede cancelar. Estatus actual: {statusActual.Clave}");

            if (statusActual.Clave == "CANCELACION_SOLICITADA")
                throw new InvalidOperationException("La cancelación ya fue solicitada.");

            // 4) Cancelar en Facturama
            const string facturamaType = "issuedLite";

            var result = await _facturama.CancelCfdiAsync(
                facturamaId: cfdi.FacturamaId,
                type: facturamaType,
                motive: motive,
                uuidReplacement: req.UuidReplacement,
                ct: ct);

            // 5) Mapear status Facturama → catálogo
            var nuevoStatusId = result.Status?.ToLowerInvariant() switch
            {
                "canceled" or "cancelled" => CfdiStatusIds.CANCELADO,
                "requested" => CfdiStatusIds.CANCELACION_SOLICITADA,
                "rejected" => CfdiStatusIds.CANCELACION_RECHAZADA,
                _ => CfdiStatusIds.ERROR_CANCELACION
            };

            // Traer la clave para guardarla en historial (porque tu historial es string)
            var nuevoStatusClave = await _context.CfdiStatuses
                .AsNoTracking()
                .Where(s => s.Id == nuevoStatusId)
                .Select(s => s.Clave)
                .FirstAsync(ct);

            // 6) Persistir en CFDI
            cfdi.CfdiStatusId = nuevoStatusId;

            cfdi.MotivoCancelacion = motive;
            cfdi.UuidSustitucion = req.UuidReplacement;

            cfdi.FechaSolicitudCancel = result.RequestDate ?? DateTime.UtcNow;
            cfdi.FechaCancelacion = result.CancelationDate;

            cfdi.EstatusCancelacionSat = result.Status;
            cfdi.AcuseCancelacionXml = result.AcuseXmlBase64;

            // (opcional) mantener columna legacy de Cfdi
            cfdi.Estatus = nuevoStatusClave.Length > 20 ? nuevoStatusClave[..20] : nuevoStatusClave;

            // 7) Insertar historial con tu estructura actual
            _context.CfdiEstatusHistorials.Add(new CfdiEstatusHistorial
            {
                // Id y CreatedAt los llena SQL por default si tu entidad lo permite (si no, asigna aquí)
                CfdiId = cfdi.Id,
                CuentaId = cuentaId,
                Estatus = nuevoStatusClave.Length > 20 ? nuevoStatusClave[..20] : nuevoStatusClave,
                Motivo = $"Cancelación SAT motivo {motive}" + (req.UuidReplacement is null ? "" : $" | Sustituye: {req.UuidReplacement:D}")
                // CreatedAt default en SQL
            });

            await _context.SaveChangesAsync(ct);

            return result;
        }

        private static string MapEstatusLocal(string? facturamaStatus)
        {
            var s = (facturamaStatus ?? "").Trim().ToLowerInvariant();
            return s switch
            {
                "canceled" or "cancelled" => "Cancelado",
                "requested" => "CancelacionSolicitada",
                "rejected" => "CancelacionRechazada",
                _ => "CancelacionSolicitada"
            };
        }

        public async Task<(byte[] bytes, string filename, string contentType)> GetAcuseCancelacionAsync(
    Guid cfdiId,
    Guid cuentaId,
    CancellationToken ct)
        {
            var cfdi = await _context.Cfdis
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == cfdiId && x.CuentaId == cuentaId, ct);

            if (cfdi is null)
                throw new KeyNotFoundException("CFDI no encontrado para la cuenta.");

            // Recomendado: solo permitir si ya está cancelado (ajusta según tu convención)
            // Si ya migraste a catálogo:
            // if (cfdi.CfdiStatusId != CfdiStatusIds.CANCELADO) throw new InvalidOperationException("El CFDI no está cancelado.");

            // Si sigues usando el string:
            if (!string.Equals(cfdi.Estatus, "Cancelado", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(cfdi.Estatus, "CANCELADO", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("El CFDI no está cancelado, no hay acuse final.");
            }

            if (string.IsNullOrWhiteSpace(cfdi.AcuseCancelacionXml))
                throw new InvalidOperationException("No hay acuse de cancelación almacenado para este CFDI.");

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(cfdi.AcuseCancelacionXml);
            }
            catch (FormatException)
            {
                // En caso de que algún día guardes el XML “raw” y no base64
                bytes = Encoding.UTF8.GetBytes(cfdi.AcuseCancelacionXml);
            }

            var filename = $"ACUSE_CANCELACION_{cfdi.Uuid:D}.xml";
            const string contentType = "application/xml";

            return (bytes, filename, contentType);
        }

        public async Task<ReenviarCfdiResponseDto> ReenviarCfdiAsync(
    Guid cfdiId,
    Guid cuentaId,
    ReenviarCfdiRequestDto req,
    CancellationToken ct = default)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var cfdi = await _context.Cfdis
                .Include(x => x.Cliente)
                .FirstOrDefaultAsync(x => x.Id == cfdiId && x.CuentaId == cuentaId, ct);

            if (cfdi is null)
                throw new KeyNotFoundException("CFDI no encontrado para la cuenta.");

            // 1) Resolver destinatario
            var to = (req.EmailTo ?? "").Trim();

            // Ajusta esto al nombre real de tu campo en Cliente
            // Ej: Cliente.Correo, Cliente.Email, Cliente.EmailFacturacion...
            if (string.IsNullOrWhiteSpace(to))
                to = (cfdi.Cliente?.Email ?? "").Trim(); // <-- CAMBIA "Correo" si tu entidad usa otro nombre

            if (string.IsNullOrWhiteSpace(to))
                throw new InvalidOperationException("No hay correo destino. Proporciona EmailTo o registra correo en el cliente.");

            _ = new MailAddress(to); // valida formato

            // 2) Adjuntos
            var attachments = new List<(byte[] bytes, string filename, string contentType)>();
            const string facturamaType = "issuedLite";

            if (req.IncludeXml)
                attachments.Add(await GetXmlAsync(cfdi.FacturamaId, facturamaType, ct));

            if (req.IncludePdf)
                attachments.Add(await GetPdfAsync(cfdi.FacturamaId, facturamaType, ct));

            if (req.IncludeAcuseCancelacion &&
                string.Equals(cfdi.Estatus, "CANCELADO", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(cfdi.AcuseCancelacionXml))
            {
                attachments.Add(await GetAcuseCancelacionAsync(cfdi.Id, cuentaId, ct));
            }

            if (attachments.Count == 0)
                throw new InvalidOperationException("No hay adjuntos seleccionados para enviar.");

            // 3) Asunto + body
            var subject = !string.IsNullOrWhiteSpace(req.Subject)
                ? req.Subject!.Trim()
                : $"CFDI {cfdi.Serie}{cfdi.Folio} - {cfdi.Uuid:D}";

            var html = BuildReenvioHtml(cfdi, req.Message);

            // 4) Enviar
            var providerId = await _emailSender.SendAsync(to, subject, html, attachments, ct);

            // 5) Registrar bitácora (tu tabla de historial ya existe)
            _context.CfdiEstatusHistorials.Add(new CfdiEstatusHistorial
            {
                CfdiId = cfdi.Id,
                CuentaId = cuentaId,
                Estatus = cfdi.Estatus, // no cambia, pero queda auditado
                Motivo = $"Reenvío por correo a {to}"
            });

            await _context.SaveChangesAsync(ct);

            return new ReenviarCfdiResponseDto
            {
                Sent = true,
                To = to,
                ProviderMessageId = providerId
            };
        }

        private static string BuildReenvioHtml(Cfdi cfdi, string? customMessage)
        {
            var msg = string.IsNullOrWhiteSpace(customMessage)
                ? "Adjuntamos su CFDI en formato PDF y XML."
                : customMessage;

            static string E(string s) => System.Net.WebUtility.HtmlEncode(s);

            return $@"
<div style=""font-family: Arial, sans-serif; font-size: 14px;"">
  <p>{E(msg)}</p>
  <hr/>
  <p><b>UUID:</b> {cfdi.Uuid:D}</p>
  <p><b>Serie/Folio:</b> {E((cfdi.Serie ?? ""))}{E((cfdi.Folio ?? ""))}</p>
  <p><b>Emisor:</b> {E(cfdi.RazonSocialEmisor ?? cfdi.RfcEmisor)}</p>
  <p><b>Receptor:</b> {E(cfdi.RazonSocialReceptor ?? cfdi.RfcReceptor)}</p>
  <p><b>Total:</b> {cfdi.Total:N2} {E(cfdi.Moneda)}</p>
  <p><b>Estatus:</b> {E(cfdi.Estatus)}</p>
</div>";
        }


        public async Task<CfdiDetalleDto> GetCfdiDetalleAsync(Guid cfdiId, Guid cuentaId, CancellationToken ct)
        {
            var cfdi = await _context.Cfdis
                .AsNoTracking()
                .Include(x => x.CfdiConceptos)
                .Include(x => x.CfdiEstatusHistorials)
                .FirstOrDefaultAsync(x => x.Id == cfdiId && x.CuentaId == cuentaId, ct);

            if (cfdi is null)
                throw new KeyNotFoundException("CFDI no encontrado.");

            var dto = new CfdiDetalleDto
            {
                Id = cfdi.Id,
                FacturamaId = cfdi.FacturamaId,
                Uuid = cfdi.Uuid,

                Serie = cfdi.Serie,
                Folio = cfdi.Folio,

                TipoCfdi = cfdi.TipoCfdi,
                Moneda = cfdi.Moneda,
                FechaTimbrado = cfdi.FechaTimbrado,

                Subtotal = cfdi.Subtotal,
                Descuento = cfdi.Descuento,
                Total = cfdi.Total,

                FormaPago = cfdi.FormaPago,
                MetodoPago = cfdi.MetodoPago,
                LugarExpedicion = cfdi.LugarExpedicion,

                RfcEmisor = cfdi.RfcEmisor,
                RazonSocialEmisor = cfdi.RazonSocialEmisor,

                RfcReceptor = cfdi.RfcReceptor,
                RazonSocialReceptor = cfdi.RazonSocialReceptor,

                Estatus = cfdi.Estatus,
                MotivoCancelacion = cfdi.MotivoCancelacion,
                UuidSustitucion = cfdi.UuidSustitucion,

                Conceptos = cfdi.CfdiConceptos
                    .OrderBy(x => x.Id)
                    .Select(x => new CfdiConceptoDto
                    {
                        Id = x.Id,
                        ProductCode = x.ClaveProdServ,   // ajusta nombres reales de tu entidad
                        UnitCode = x.ClaveUnidad,
                        Cantidad = x.Cantidad,
                        Unidad = x.Unidad,
                        Descripcion = x.Descripcion,
                        ValorUnitario = x.ValorUnitario,
                        Descuento = x.Descuento,
                        Importe = x.Total,
                    })
                    .ToList(),

                Historial = cfdi.CfdiEstatusHistorials
                    .OrderByDescending(h => h.CreatedAt)
                    .Select(h => new CfdiHistorialDto
                    {
                        Id = h.Id,
                        Estatus = h.Estatus,
                        Motivo = h.Motivo,
                        CreatedAt = h.CreatedAt
                    })
                    .ToList()
            };

            return dto;
        }


        private static void ValidarNcTotal(FacturamaCfdiRequest req)
        {
            if (req.CfdiType != "E")
                throw new InvalidOperationException("NC debe ser CfdiType=E.");

            if (req.NameId != "2")
                throw new InvalidOperationException("NC requiere NameId='2' (Facturama).");

            if (req.Relations is null || req.Relations.Cfdis.Count == 0 || req.Relations.Cfdis.Any(x => string.IsNullOrWhiteSpace(x.Uuid)))
                throw new InvalidOperationException("NC requiere Relations con UUID origen.");

            if (req.Items is null || req.Items.Count == 0)
                throw new InvalidOperationException("NC requiere al menos 1 concepto.");

            if (req.Issuer is null || string.IsNullOrWhiteSpace(req.Issuer.FiscalRegime))
                throw new InvalidOperationException("Falta FiscalRegime del emisor.");

            if (req.Receiver is null || string.IsNullOrWhiteSpace(req.Receiver.FiscalRegime) || string.IsNullOrWhiteSpace(req.Receiver.TaxZipCode))
                throw new InvalidOperationException("Faltan FiscalRegime/TaxZipCode del receptor.");
        }


        public async Task<CfdiCreadoDto> CrearNotaCreditoTotalAsync(Guid cfdiId, CancellationToken ct)
        {
            var origen = await _context.Cfdis
                .Include(x => x.CfdiConceptos)
                    .ThenInclude(c => c.CfdiConceptoImpuestos)
                .FirstOrDefaultAsync(x => x.Id == cfdiId, ct);

            if (origen is null)
                throw new InvalidOperationException("CFDI origen no encontrado.");

            if (!string.Equals(origen.TipoCfdi, "I", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Solo se puede generar NC desde CFDI de Ingreso (I).");

            if (!string.Equals(origen.Estatus, "TIMBRADO", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("El CFDI origen debe estar TIMBRADO.");

            // Armar payload NC TOTAL
            var payload = new FacturamaCfdiRequest
            {
                Date = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                Serie = origen.Serie,
                Folio = origen.Folio,
                CfdiType = "E",
                NameId = "2",

                Currency = origen.Moneda,
                ExpeditionPlace = origen.LugarExpedicion,
                Exportation = string.IsNullOrWhiteSpace(origen.Exportacion) ? "01" : origen.Exportacion,

                PaymentForm = "99",
                PaymentMethod = "PUE",

                Issuer = new FacturamaIssuer
                {
                    Rfc = origen.RfcEmisor,
                    Name = origen.RazonSocialEmisor ?? "EMISOR",
                    FiscalRegime = origen.EmisorRegimenFiscal ?? throw new InvalidOperationException("Falta EmisorRegimenFiscal en CFDI origen.")
                },

                Receiver = new FacturamaReceiver
                {
                    Rfc = origen.RfcReceptor,
                    Name = origen.RazonSocialReceptor ?? "RECEPTOR",
                    CfdiUse = "G02", // NC
                    FiscalRegime = origen.ReceptorRegimenFiscal ?? throw new InvalidOperationException("Falta ReceptorRegimenFiscal en CFDI origen."),
                    TaxZipCode = origen.ReceptorTaxZipCode ?? throw new InvalidOperationException("Falta ReceptorTaxZipCode en CFDI origen.")
                },

                Relations = new FacturamaRelations
                {
                    Type = "01",
                    Cfdis = new List<FacturamaRelatedCfdi>
            {
                new() { Uuid = origen.Uuid.ToString() }
            }
                },

                Items = origen.CfdiConceptos.Select(con => new FacturamaItem
                {
                    ProductCode = con.ClaveProdServ,
                    UnitCode = con.ClaveUnidad ?? "H87",
                    Description = con.Descripcion,
                    Quantity = con.Cantidad,
                    UnitPrice = con.ValorUnitario,
                    Discount = con.Descuento,
                    Unit = con.Unidad,
                    IdentificationNumber = con.NoIdentificacion,
                    Subtotal = con.Subtotal,
                    TaxObject = con.ObjetoImp,
                    Taxes = con.CfdiConceptoImpuestos.Select(imp => new FacturamaTax
                    {
                        Name = imp.TipoImpuesto,
                        Rate = imp.Tasa,
                        Base = imp.Base,
                        Total = imp.Importe,
                        IsRetention = imp.EsRetencion
                    }).ToList(),
                    Total = con.Total
                }).ToList()
            };

            // Llamada Facturama (tu método actual)
            var requestJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = null });
            var (doc, responseJson) = await _facturama.CrearCfdiMultiRawAsync(payload, ct);

            // EmitirCfdiRequest mínimo para reusar GuardarCfdiAsync
            var req = new EmitirCfdiRequest
            {
                ClienteId = origen.ClienteId,
                Serie = payload.Serie,
                Folio = payload.Folio,
                CfdiType = "E",
                Currency = payload.Currency,
                PaymentForm = payload.PaymentForm,
                PaymentMethod = payload.PaymentMethod,
                ExpeditionPlace = payload.ExpeditionPlace
            };

            // ✅ Reusar tu guardado (con relación)
            await GuardarCfdiAsync(
                cuentaId: origen.CuentaId,
                req: req,
                payload: payload,
                requestJson: requestJson,
                responseJson: responseJson,
                facturamaDoc: doc,
                ct: ct,
                cfdiOrigenId: origen.Id,
                tipoRelacionSat: "01"
            );

            // Si quieres devolver el Id/UUID recién creado, aquí hay 2 opciones:
            // A) Modificar GuardarCfdiAsync para que regrese el Cfdi.Id creado
            // B) Buscar por FacturamaId/Uuid parseando doc (rápido)
            var uuidNc = Guid.Parse(doc.RootElement.GetProperty("Complement").GetProperty("TaxStamp").GetProperty("Uuid").GetString()!);

            var nc = await _context.Cfdis
                .AsNoTracking()
                .Where(x => x.Uuid == uuidNc)
                .Select(x => new CfdiCreadoDto
                {
                    Id = x.Id,
                    Uuid = x.Uuid.ToString(),
                    FacturamaId = x.FacturamaId,
                    Serie = x.Serie,
                    Folio = x.Folio,
                    Total = x.Total
                })
                .FirstAsync(ct);

            return nc;
        }
    }
}
