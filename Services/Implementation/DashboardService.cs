using Facturacion.API.Data;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto.Cliente.Dashboard;
using Facturacion.API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Facturacion.API.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private static readonly CultureInfo _mx = new("es-MX");
        private static string Money(decimal v) => v.ToString("C2", _mx);
        private static string Num(int v) => v.ToString("N0", _mx);
        private static readonly string[] _mesesCorto = new[]
        {
            "Ene","Feb","Mar","Abr","May","Jun","Jul","Ago","Sep","Oct","Nov","Dic"
        };

        private static DateTime FirstDayOfMonth(DateTime d) => new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly FacturacionContext _context;

        public DashboardService(FacturacionContext context)
        {
            _context = context;
        }

        private static DashboardTrendDto Trend(decimal cur, decimal prev)
        {
            if (prev == 0m)
            {
                if (cur == 0m) return new DashboardTrendDto { Direction = "up", Value = "+0.0%", Note = "vs. periodo anterior" };
                return new DashboardTrendDto { Direction = "up", Value = "+100.0%", Note = "vs. periodo anterior" };
            }

            var pct = (cur - prev) / prev * 100m;
            return new DashboardTrendDto
            {
                Direction = pct >= 0 ? "up" : "down",
                Value = $"{(pct >= 0 ? "+" : "")}{Math.Round(pct, 1, MidpointRounding.AwayFromZero):0.0}%",
                Note = "vs. periodo anterior"
            };
        }

        private static DashboardTrendDto Trend(int cur, int prev)
        {
            if (prev == 0)
            {
                if (cur == 0) return new DashboardTrendDto { Direction = "up", Value = "+0.0%", Note = "vs. periodo anterior" };
                return new DashboardTrendDto { Direction = "up", Value = "+100.0%", Note = "vs. periodo anterior" };
            }

            var pct = ((decimal)cur - prev) / prev * 100m;
            return new DashboardTrendDto
            {
                Direction = pct >= 0 ? "up" : "down",
                Value = $"{(pct >= 0 ? "+" : "")}{Math.Round(pct, 1, MidpointRounding.AwayFromZero):0.0}%",
                Note = "vs. periodo anterior"
            };
        }


        private IQueryable<Cfdi> QueryBaseCfdi(Guid cuentaId, DashboardRequest req)
        {
            var q = _context.Cfdis.AsNoTracking()
                .Where(x => x.CuentaId == cuentaId);

            if (req.SucursalId.HasValue)
                q = q.Where(x => x.SucursalId == req.SucursalId.Value);

            if (req.RazonSocialId.HasValue)
                q = q.Where(x => x.RazonSocialId == req.RazonSocialId.Value);

            if (!string.IsNullOrWhiteSpace(req.Moneda) &&
                !req.Moneda.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var m = req.Moneda.Trim().ToUpperInvariant();
                q = q.Where(x => x.Moneda == m);
            }

            return q;
        }

        private async Task<List<DashboardKpiDto>> GetKpisAsync(Guid cuentaId, DashboardRequest req, CancellationToken ct)
        {
            var hasta = req.Hasta ?? DateTime.UtcNow;
            var desde = req.Desde ?? hasta.AddDays(-30);

            var span = hasta - desde;
            if (span.TotalMinutes <= 0) span = TimeSpan.FromDays(30);

            var prevHasta = desde;
            var prevDesde = desde - span;

            var baseQ = QueryBaseCfdi(cuentaId, req);

            // ⚠️ AJUSTA el campo de fecha:
            var qCur = baseQ.Where(x => x.FechaTimbrado >= desde && x.FechaTimbrado < hasta);
            var qPrev = baseQ.Where(x => x.FechaTimbrado >= prevDesde && x.FechaTimbrado < prevHasta);

            // 1) Facturado (I timbrado)
            var factCur = await qCur
                .Where(x => x.TipoCfdi == "I" && x.Estatus == "TIMBRADO")
                .SumAsync(x => (decimal?)x.Total, ct) ?? 0m;

            var factPrev = await qPrev
                .Where(x => x.TipoCfdi == "I" && x.Estatus == "TIMBRADO")
                .SumAsync(x => (decimal?)x.Total, ct) ?? 0m;

            // 2) CFDIs emitidos (timbrados)
            var emitCur = await qCur.Where(x => x.Estatus == "TIMBRADO").CountAsync(ct);
            var emitPrev = await qPrev.Where(x => x.Estatus == "TIMBRADO").CountAsync(ct);

            // 3) Cancelaciones (si tienes FechaCancelacion, te lo ajusto)
            var cancCur = await qCur.Where(x => x.Estatus == "CANCELADO").CountAsync(ct);
            var cancPrev = await qPrev.Where(x => x.Estatus == "CANCELADO").CountAsync(ct);

            // 4) Notas de crédito (E timbrado)
            var ncCur = await qCur.Where(x => x.TipoCfdi == "E" && x.Estatus == "TIMBRADO").CountAsync(ct);
            var ncPrev = await qPrev.Where(x => x.TipoCfdi == "E" && x.Estatus == "TIMBRADO").CountAsync(ct);

            var monedaLabel = string.IsNullOrWhiteSpace(req.Moneda) || req.Moneda.Equals("ALL", StringComparison.OrdinalIgnoreCase)
                ? "MXN"
                : req.Moneda.Trim().ToUpperInvariant();

            return new List<DashboardKpiDto>
    {
        new()
        {
            Title = $"Facturado ({monedaLabel})",
            Value = Money(factCur),
            Hint  = "Total de Ingresos (I) timbrados en el periodo",
            Trend = Trend(factCur, factPrev),
            Icon  = "paid"
        },
        new()
        {
            Title = "CFDIs emitidos",
            Value = Num(emitCur),
            Hint  = "CFDIs timbrados en el periodo (I/E/P)",
            Trend = Trend(emitCur, emitPrev),
            Icon  = "receipt_long"
        },
        new()
        {
            Title = "Cancelaciones",
            Value = Num(cancCur),
            Hint  = "CFDIs cancelados en el periodo",
            Trend = Trend(cancCur, cancPrev),
            Icon  = "block"
        },
        new()
        {
            Title = "Notas de crédito",
            Value = Num(ncCur),
            Hint  = "CFDIs de Egreso (E) timbrados en el periodo",
            Trend = Trend(ncCur, ncPrev),
            Icon  = "note_alt"
        }
    };
        }
        private async Task<List<DashboardMonthlyPointDto>> GetFacturacion12mAsync(Guid cuentaId, DashboardRequest req, CancellationToken ct)
        {
            var meses = req.MesesHistorico <= 0 ? 12 : Math.Min(req.MesesHistorico, 24); // limita por seguridad
            var hasta = req.Hasta ?? DateTime.UtcNow;

            // rango: desde el primer día del mes (meses-1 meses atrás) hasta el primer día del mes siguiente a "hasta"
            var endMonthStart = new DateTime(hasta.Year, hasta.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
            var startMonthStart = new DateTime(hasta.Year, hasta.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(meses - 1));

            // Query base con filtros
            var q = _context.Cfdis.AsNoTracking()
                .Where(x => x.CuentaId == cuentaId);

            if (req.SucursalId.HasValue)
                q = q.Where(x => x.SucursalId == req.SucursalId.Value);

            if (req.RazonSocialId.HasValue)
                q = q.Where(x => x.RazonSocialId == req.RazonSocialId.Value);

            if (!string.IsNullOrWhiteSpace(req.Moneda) &&
                !req.Moneda.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var m = req.Moneda.Trim().ToUpperInvariant();
                q = q.Where(x => x.Moneda == m);
            }

            // ⚠️ Ajusta el campo de fecha aquí:
            q = q.Where(x => x.FechaTimbrado >= startMonthStart && x.FechaTimbrado < endMonthStart);

            // Solo ingresos timbrados
            q = q.Where(x => x.TipoCfdi == "I" && x.Estatus == "TIMBRADO");

            // Agrupar por año/mes
            var rows = await q
                .GroupBy(x => new { x.FechaTimbrado.Year, x.FechaTimbrado.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(x => x.Total)
                })
                .ToListAsync(ct);

            // Index para lookup
            var map = rows.ToDictionary(x => (x.Year, x.Month), x => x.Amount);

            // Ensamblar N meses consecutivos (incluyendo ceros)
            var result = new List<DashboardMonthlyPointDto>(meses);

            for (int i = 0; i < meses; i++)
            {
                var d = startMonthStart.AddMonths(i);
                var y = d.Year;
                var m = d.Month;

                map.TryGetValue((y, m), out var amount);

                result.Add(new DashboardMonthlyPointDto
                {
                    Year = y,
                    Month = m,
                    Label = _mesesCorto[m - 1],
                    Amount = amount
                });
            }

            return result;
        }

        private async Task<List<DashboardStatusSliceDto>> GetEstatusAsync(
    Guid cuentaId,
    DashboardRequest req,
    CancellationToken ct)
        {
            var hasta = req.Hasta ?? DateTime.UtcNow;
            var desde = req.Desde ?? hasta.AddDays(-30);

            var q = _context.Cfdis.AsNoTracking()
                .Where(x => x.CuentaId == cuentaId);

            if (req.SucursalId.HasValue)
                q = q.Where(x => x.SucursalId == req.SucursalId.Value);

            if (req.RazonSocialId.HasValue)
                q = q.Where(x => x.RazonSocialId == req.RazonSocialId.Value);

            if (!string.IsNullOrWhiteSpace(req.Moneda) &&
                !req.Moneda.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var m = req.Moneda.Trim().ToUpperInvariant();
                q = q.Where(x => x.Moneda == m);
            }

            // ⚠️ Ajusta el campo de fecha aquí:
            q = q.Where(x => x.FechaTimbrado >= desde && x.FechaTimbrado < hasta);

            // Conteos por estatus
            var rows = await q
                .GroupBy(x => x.Estatus)
                .Select(g => new { Estatus = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            int CountOf(string s) =>
                rows.FirstOrDefault(r => r.Estatus == s)?.Count ?? 0;

            var timbrado = CountOf("TIMBRADO");
            var cancelado = CountOf("CANCELADO");
            var borrador = CountOf("BORRADOR");
            var error = CountOf("ERROR");

            var total = timbrado + cancelado + borrador + error;

            // Si no hay datos, regresamos 0% en todo
            if (total == 0)
            {
                return new List<DashboardStatusSliceDto>
        {
            new() { Status = "TIMBRADO", Label = "Timbrado", Value = 0 },
            new() { Status = "CANCELADO", Label = "Cancelado", Value = 0 },
            new() { Status = "BORRADOR", Label = "Borrador", Value = 0 },
            new() { Status = "ERROR", Label = "Error", Value = 0 }
        };
            }

            int Pct(int c) => (int)Math.Round((decimal)c * 100m / total, 0, MidpointRounding.AwayFromZero);

            // Porcentajes con ajuste para que sumen 100
            var pTim = Pct(timbrado);
            var pCan = Pct(cancelado);
            var pBor = Pct(borrador);
            var pErr = Pct(error);

            var sum = pTim + pCan + pBor + pErr;
            if (sum != 100)
            {
                // Ajuste simple: corrige el mayor bucket (normalmente TIMBRADO)
                // Si prefieres siempre ajustar TIMBRADO, cambia el if.
                var diff = 100 - sum;

                // Encuentra el bucket con mayor conteo para ajustar
                var buckets = new List<(string key, int count)>
        {
            ("TIMBRADO", timbrado),
            ("CANCELADO", cancelado),
            ("BORRADOR", borrador),
            ("ERROR", error)
        }.OrderByDescending(x => x.count).ToList();

                var top = buckets.First().key;
                if (top == "TIMBRADO") pTim += diff;
                else if (top == "CANCELADO") pCan += diff;
                else if (top == "BORRADOR") pBor += diff;
                else pErr += diff;
            }

            return new List<DashboardStatusSliceDto>
    {
        new() { Status = "TIMBRADO", Label = "Timbrado", Value = pTim },
        new() { Status = "CANCELADO", Label = "Cancelado", Value = pCan },
        new() { Status = "BORRADOR", Label = "Borrador", Value = pBor },
        new() { Status = "ERROR", Label = "Error", Value = pErr }
    };
        }

        private async Task<List<DashboardRecentCfdiDto>> GetRecientesAsync(
    Guid cuentaId,
    DashboardRequest req,
    CancellationToken ct)
        {
            var hasta = req.Hasta ?? DateTime.UtcNow;
            var desde = req.Desde ?? hasta.AddDays(-30);

            var take = req.TakeRecientes <= 0 ? 10 : Math.Min(req.TakeRecientes, 50);

            var q = _context.Cfdis.AsNoTracking()
                .Where(x => x.CuentaId == cuentaId);

            if (req.SucursalId.HasValue)
                q = q.Where(x => x.SucursalId == req.SucursalId.Value);

            if (req.RazonSocialId.HasValue)
                q = q.Where(x => x.RazonSocialId == req.RazonSocialId.Value);

            if (!string.IsNullOrWhiteSpace(req.Moneda) &&
                !req.Moneda.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var m = req.Moneda.Trim().ToUpperInvariant();
                q = q.Where(x => x.Moneda == m);
            }

            // ⚠️ Ajusta el campo de fecha:
            // Ideal: para recientes usa FechaTimbrado si existe; si no, FechaCreacion.
            q = q.Where(x => x.FechaTimbrado >= desde && x.FechaTimbrado < hasta);

            // Orden: más reciente primero (si FechaTimbrado puede ser null, usa coalesce)
            var rows = await q
                .OrderByDescending(x => x.FechaTimbrado)
                .ThenByDescending(x => x.Id)
                .Select(x => new DashboardRecentCfdiDto
                {
                    Id = x.Id,
                    Uuid = x.Uuid.ToString(),

                    Fecha = x.FechaTimbrado, // ⚠️ ajusta si tu campo se llama distinto

                    Serie = x.Serie ?? "",
                    Folio = x.Folio ?? "",

                    Receptor = (x.RazonSocialReceptor ?? "RECEPTOR"),
                    Rfc = x.RfcReceptor ?? "",

                    Total = x.Total,
                    Moneda = x.Moneda ?? "MXN",

                    Estatus = x.Estatus ?? "",
                    TipoCfdi = x.TipoCfdi
                })
                .Take(take)
                .ToListAsync(ct);

            return rows;
        }

        private async Task<List<DashboardTopClientDto>> GetTopClientesAsync(Guid cuentaId, DashboardRequest req, CancellationToken ct)
        {
            var hasta = req.Hasta ?? DateTime.UtcNow;
            var desde = req.Desde ?? hasta.AddDays(-30);

            var top = req.TakeTopClientes <= 0 ? 5 : Math.Min(req.TakeTopClientes, 50);

            var q = _context.Cfdis.AsNoTracking()
                .Where(x => x.CuentaId == cuentaId);

            if (req.SucursalId.HasValue)
                q = q.Where(x => x.SucursalId == req.SucursalId.Value);

            if (req.RazonSocialId.HasValue)
                q = q.Where(x => x.RazonSocialId == req.RazonSocialId.Value);

            if (!string.IsNullOrWhiteSpace(req.Moneda) &&
                !req.Moneda.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var m = req.Moneda.Trim().ToUpperInvariant();
                q = q.Where(x => x.Moneda == m);
            }

            // ⚠️ Ajusta el campo de fecha aquí
            q = q.Where(x => x.FechaTimbrado >= desde && x.FechaTimbrado < hasta);

            // Solo ingresos timbrados (top clientes por facturación)
            q = q.Where(x => x.TipoCfdi == "I" && x.Estatus == "TIMBRADO");

            // Agrupar por ClienteId si existe; si no, por RFC+Nombre
            // Si tu Cfdi SIEMPRE tiene ClienteId, puedes simplificar.
            var rows = await q
                .GroupBy(x => new
                {
                    x.ClienteId,
                    Rfc = x.RfcReceptor,
                    Nombre = x.RazonSocialReceptor
                })
                .Select(g => new DashboardTopClientDto
                {
                    ClienteId = g.Key.ClienteId,
                    Rfc = g.Key.Rfc ?? "",
                    Nombre = g.Key.Nombre ?? "RECEPTOR",
                    Facturas = g.Count(),
                    Total = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.Total)
                .ThenByDescending(x => x.Facturas)
                .Take(top)
                .ToListAsync(ct);

            // (opcional) si quieres limpiar nombres vacíos
            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.Rfc)) r.Rfc = "—";
                if (string.IsNullOrWhiteSpace(r.Nombre)) r.Nombre = "RECEPTOR";
            }

            return rows;
        }

        public async Task<DashboardDto> GetDashboardAsync(Guid cuentaId, DashboardRequest req, CancellationToken ct)
        {
            try
            {
                var dto = new DashboardDto
                {
                    Kpis = await GetKpisAsync(cuentaId, req, ct),
                    Facturacion12m = await GetFacturacion12mAsync(cuentaId, req, ct),
                    StatusSlices = await GetEstatusAsync(cuentaId, req, ct),
                    RecentCfdis = await GetRecientesAsync(cuentaId, req, ct),
                    TopClients = await GetTopClientesAsync(cuentaId, req, ct),
                    Alerts = new List<DashboardAlertDto>()
                };
                return dto;
            }
            catch (Exception ex) {
                throw;
            }
            
        }
    }
}


/*
 Kpis = await GetKpisAsync(cuentaId, req, ct),
        Facturacion12m = await GetFacturacion12mAsync(cuentaId, req, ct),
        Estatus = await GetEstatusAsync(cuentaId, req, ct),
        Recientes = await GetRecientesAsync(cuentaId, req, ct),
        TopClientes = await GetTopClientesAsync(cuentaId, req, ct),
        Alertas = await GetAlertasAsync(cuentaId, req, ct)
 
 */