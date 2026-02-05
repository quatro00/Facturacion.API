using Facturacion.API.Data;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto.Cliente.Sucursal;
using Facturacion.API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace Facturacion.API.Services.Implementation
{
    public class SucursalService : ISucursalService
    {
        private readonly FacturacionContext _context;

        public SucursalService(FacturacionContext context)
        {
            _context = context;
            //_user = user;
        }

        public async Task<Guid> CreateAsync(SucursalCreateDto dto, Guid cuentaId, string usuarioId, CancellationToken ct)
        {
            /*
            var cuentaId = cuentaId;      // Guid
            var usuarioId = usuarioId;    // string (nvarchar(50))
            */
            // ================
            // Validaciones base
            // ================
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Codigo)) throw new InvalidOperationException("Código requerido.");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new InvalidOperationException("Nombre requerido.");

            if (dto.RazonesSociales is null || dto.RazonesSociales.Count == 0)
                throw new InvalidOperationException("Selecciona al menos una razón social.");

            // Normaliza
            var codigo = dto.Codigo.Trim().ToUpperInvariant();
            var nombre = dto.Nombre.Trim();

            // Código único por cuenta (recomendado)
            var existsCodigo = await _context.Sucursals
                .AnyAsync(x => x.CuentaId == cuentaId && !x.IsDeleted && x.Codigo == codigo, ct);

            if (existsCodigo)
                throw new InvalidOperationException($"Ya existe una sucursal con el código '{codigo}'.");

            // Validar que las razones sociales existan y pertenezcan a la cuenta
            var rsIds = dto.RazonesSociales.Select(x => x.RazonSocialId).Distinct().ToList();

            var rsCount = await _context.RazonSocials
                .Where(x => x.CuentaId == cuentaId && rsIds.Contains(x.Id)).CountAsync(ct);

            if (rsCount != rsIds.Count)
                throw new InvalidOperationException("Una o más razones sociales no existen o no pertenecen a la cuenta.");

            // =====================
            // Crear entidad Sucursal
            // =====================
            var sucursalId = Guid.NewGuid();

            var sucursal = new Sucursal
            {
                Id = sucursalId,
                CuentaId = cuentaId,

                Codigo = codigo,
                Nombre = nombre,

                Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),

                CodigoPostal = dto.Direccion?.CodigoPostal?.Trim(),
                Municipio = dto.Direccion?.Municipio?.Trim(),
                Estado = dto.Direccion?.Estado?.Trim(),
                Colonia = dto.Direccion?.Colonia?.Trim(),
                Calle = dto.Direccion?.Calle?.Trim(),
                NoInterior = dto.Direccion?.NoInterior?.Trim(),
                NoExterior = dto.Direccion?.NoExterior?.Trim(),

                Activo = dto.Activo,
                IsDeleted = false,

                FechaCreacion = DateTime.Now,
                UsuarioCreacionId = usuarioId,
            };

            _context.Sucursals.Add(sucursal);

            // ======================================================
            // Relación SucursalRazonSocial + regla de default único
            // ======================================================

            // Si quieres permitir varios defaults (no recomendado), quita este bloque.
            var defaultsSolicitados = dto.RazonesSociales.Where(x => x.EsDefault).Select(x => x.RazonSocialId).Distinct().ToList();
            if (defaultsSolicitados.Count > 0)
            {
                var existentesDefault = await _context.SucursalRazonSocials
                    .Where(x => x.CuentaId == cuentaId
                                && (x.Activo)
                                && x.EsDefault
                                && defaultsSolicitados.Contains(x.RazonSocialId))
                    .ToListAsync(ct);

                foreach (var rel in existentesDefault)
                {
                    rel.EsDefault = false;
                    // (opcional) auditoría de modificación si la tienes en esa tabla
                }
            }

            var ahora = DateTime.Now;

            foreach (var rs in dto.RazonesSociales)
            {
                _context.SucursalRazonSocials.Add(new SucursalRazonSocial
                {
                    Id = Guid.NewGuid(),
                    CuentaId = cuentaId,
                    SucursalId = sucursalId,
                    RazonSocialId = rs.RazonSocialId,
                    Activo = rs.Activo,
                    EsDefault = rs.EsDefault,
                    FechaCreacion = ahora,
                    UsuarioCreacionId = usuarioId
                });
            }

            await _context.SaveChangesAsync(ct);

            return sucursalId;
        }

        public async Task<PagedResult<SucursalRowDto>> GetSucursalesAsync(
        Guid cuentaId,
        string usuarioId,
        SucursalesQueryDto queryDto,
        CancellationToken ct
    )
        {
            // usuarioId queda disponible por si luego auditas accesos, etc.
            _ = usuarioId;

            // Normaliza y límites
            var q = queryDto.Q?.Trim();
            var status = (queryDto.Status ?? "ALL").Trim().ToUpperInvariant();

            var page = queryDto.Page <= 0 ? 1 : queryDto.Page;
            var pageSize = queryDto.PageSize <= 0 ? 10 : queryDto.PageSize;
            if (pageSize > 200) pageSize = 200;

            var sort = (queryDto.Sort ?? "codigo").Trim().ToLowerInvariant();
            var dir = (queryDto.Dir ?? "asc").Trim().ToLowerInvariant();
            var isDesc = dir == "desc";

            // Base query
            IQueryable<Sucursal> query = _context.Sucursals
                .AsNoTracking()
                .Where(s => s.CuentaId == cuentaId && !s.IsDeleted);

            // 🔎 Search
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(s => s.Codigo.Contains(q) || s.Nombre.Contains(q));
            }

            // 🔵 Status
            if (status == "ACTIVE") query = query.Where(s => s.Activo);
            else if (status == "INACTIVE") query = query.Where(s => !s.Activo);

            // 🏢 Filtro por razón social
            if (queryDto.RazonSocialId.HasValue)
            {
                var rsId = queryDto.RazonSocialId.Value;
                query = query.Where(s =>
                    _context.SucursalRazonSocials.Any(rel =>
                        rel.CuentaId == cuentaId &&
                        rel.Activo &&
                        rel.SucursalId == s.Id &&
                        rel.RazonSocialId == rsId));
            }

            // Total antes de paginar
            var total = await query.CountAsync(ct);

            // 🧭 Sorting
            query = sort switch
            {
                "nombre" => isDesc ? query.OrderByDescending(x => x.Nombre) : query.OrderBy(x => x.Nombre),
                "activo" => isDesc ? query.OrderByDescending(x => x.Activo) : query.OrderBy(x => x.Activo),
                "fechacreacion" => isDesc ? query.OrderByDescending(x => x.FechaCreacion) : query.OrderBy(x => x.FechaCreacion),
                _ => isDesc ? query.OrderByDescending(x => x.Codigo) : query.OrderBy(x => x.Codigo),
            };

            var skip = (page - 1) * pageSize;

            // Page de sucursales
            var pageSucursales = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(s => new SucursalRowDto
                {
                    Id = s.Id,
                    Codigo = s.Codigo,
                    Nombre = s.Nombre,
                    Activo = s.Activo,
                    Municipio = s.Municipio,
                    Estado = s.Estado,
                    Cp = s.CodigoPostal,
                    RazonesSociales = new List<SucursalRazonDto>()
                })
                .ToListAsync(ct);

            if (pageSucursales.Count == 0)
            {
                return new PagedResult<SucursalRowDto>
                {
                    Total = total,
                    Items = pageSucursales
                };
            }

            // Traer relaciones + razón social (1 query adicional)
            var sucursalIds = pageSucursales.Select(x => x.Id).ToList();

            var rels = await (
                from rel in _context.SucursalRazonSocials.AsNoTracking()
                join rs in _context.RazonSocials.AsNoTracking()
                    on rel.RazonSocialId equals rs.Id
                where rel.CuentaId == cuentaId
                      && rel.Activo
                      && sucursalIds.Contains(rel.SucursalId)
                select new
                {
                    rel.SucursalId,
                    RazonSocialId = rs.Id,
                    rs.Rfc,
                    RazonSocial = rs.RazonSocial1, // ajusta si tu columna se llama distinto
                    rel.EsDefault
                }
            ).ToListAsync(ct);

            // Map a cada sucursal
            var dict = pageSucursales.ToDictionary(x => x.Id);

            foreach (var r in rels)
            {
                if (!dict.TryGetValue(r.SucursalId, out var s)) continue;

                s.RazonesSociales.Add(new SucursalRazonDto
                {
                    Id = r.RazonSocialId,
                    Rfc = r.Rfc,
                    RazonSocial = r.RazonSocial,
                    EsDefault = r.EsDefault
                });
            }

            return new PagedResult<SucursalRowDto>
            {
                Total = total,
                Items = pageSucursales
            };
        }

        public async Task<ToggleActivoResponse_Sucursal> SetActivoAsync(
        Guid cuentaId,
        string usuarioId,
        Guid sucursalId,
        bool activo,
        CancellationToken ct)
        {
            // Traer sucursal (con tracking porque vamos a actualizar)
            var sucursal = await _context.Sucursals
                .FirstOrDefaultAsync(x => x.Id == sucursalId && x.CuentaId == cuentaId && !x.IsDeleted, ct);

            if (sucursal is null)
                throw new InvalidOperationException("Sucursal no encontrada.");

            // No hacer update innecesario
            if (sucursal.Activo == activo)
            {
                return new ToggleActivoResponse_Sucursal
                {
                    Id = sucursal.Id,
                    Activo = sucursal.Activo,
                    FechaModificacion = sucursal.FechaModificacion ?? sucursal.FechaCreacion
                };
            }

            sucursal.Activo = activo;
            sucursal.FechaModificacion = DateTime.Now;
            sucursal.UsuarioModificacionId = usuarioId;

            await _context.SaveChangesAsync(ct);

            return new ToggleActivoResponse_Sucursal
            {
                Id = sucursal.Id,
                Activo = sucursal.Activo,
                FechaModificacion = sucursal.FechaModificacion ?? DateTime.Now
            };
        }


        public async Task<SucursalDetalleDto> GetSucursalByIdAsync(Guid cuentaId, Guid sucursalId, CancellationToken ct)
        {
            var s = await _context.Sucursals
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == sucursalId && x.CuentaId == cuentaId && !x.IsDeleted, ct);

            if (s is null)
                throw new InvalidOperationException("Sucursal no encontrada.");

            // Razones sociales asociadas (solo activas según tu regla actual)
            var rels = await (
                from rel in _context.SucursalRazonSocials.AsNoTracking()
                join rs in _context.RazonSocials.AsNoTracking() on rel.RazonSocialId equals rs.Id
                where rel.CuentaId == cuentaId
                      && rel.SucursalId == sucursalId
                      && rel.Activo
                select new SucursalDetalleRazonDto
                {
                    Id = rs.Id,
                    Rfc = rs.Rfc,
                    RazonSocial = rs.RazonSocial1, // ajusta si el campo se llama distinto
                    EsDefault = rel.EsDefault,
                    Activo = rel.Activo
                }
            ).ToListAsync(ct);

            // ✅ Orden fijo de tus 7 conceptos (para que el UI pinte igual siempre)
            var conceptosOrden = new[]
            {
        "I_MERCANCIAS",
        "I_SERVICIOS",
        "I_ANTICIPO",
        "E_DESCUENTOS",
        "E_DEVOLUCIONES",
        "E_APLICACION_ANTICIPO",
        "P_RECIBOS_PAGO",
    };

            // ✅ Series por sucursal (ya incluye Concepto)
            var seriesDb = await _context.SucursalSeries
                .AsNoTracking()
                .Where(x => x.CuentaId == cuentaId && x.SucursalId == sucursalId)
                .Select(x => new SucursalSerieDto
                {
                    Concepto = x.Concepto,  // ✅ NUEVO
                    TipoCfdi = (!string.IsNullOrWhiteSpace(x.TipoCfdi) ? char.ToUpperInvariant(x.TipoCfdi.Trim()[0]) : '?'),
                    Serie = x.Serie ?? "",
                    FolioActual = x.FolioActual,
                    Activo = x.Activo
                })
                .ToListAsync(ct);

            // ✅ Completar faltantes para asegurar 7 conceptos siempre
            // (Esto le facilita la vida al front porque el FormArray es fijo)
            var map = seriesDb
                .Where(x => !string.IsNullOrWhiteSpace(x.Concepto))
                .ToDictionary(x => x.Concepto.Trim().ToUpperInvariant(), x => x);

            var series = new List<SucursalSerieDto>(conceptosOrden.Length);

            foreach (var c in conceptosOrden)
            {
                if (map.TryGetValue(c, out var row))
                {
                    // Asegurar que TipoCfdi esté bien según el prefijo del concepto
                    // (Opcional, por si viene mal guardado)
                    row.TipoCfdi = c.StartsWith("I_") ? 'I' : c.StartsWith("E_") ? 'E' : c.StartsWith("P_") ? 'P' : row.TipoCfdi;
                    row.Concepto = c;
                    series.Add(row);
                }
                else
                {
                    // No existe aún en DB -> fila vacía
                    series.Add(new SucursalSerieDto
                    {
                        Concepto = c,
                        TipoCfdi = c.StartsWith("I_") ? 'I' : c.StartsWith("E_") ? 'E' : 'P',
                        Serie = "",
                        FolioActual = 0,
                        Activo = true
                    });
                }
            }

            return new SucursalDetalleDto
            {
                Id = s.Id,
                Codigo = s.Codigo,
                Nombre = s.Nombre,
                Activo = s.Activo,

                Telefono = s.Telefono,
                Email = s.Email,

                CodigoPostal = s.CodigoPostal,
                Municipio = s.Municipio,
                Estado = s.Estado,
                Colonia = s.Colonia,
                Calle = s.Calle,
                NoInterior = s.NoInterior,
                NoExterior = s.NoExterior,

                RowVersion = Convert.ToBase64String(s.RowVersion),

                RazonesSociales = rels,

                // ✅ Aquí van las 7 series en orden fijo
                Series = series
            };
        }


        public async Task<SerieFolioPreviewDto> GetSerieFolioPreviewAsync(
    Guid cuentaId,
    Guid sucursalId,
    string conceptoSerie,
    CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(conceptoSerie))
                throw new InvalidOperationException("conceptoSerie es requerido.");

            conceptoSerie = conceptoSerie.Trim().ToUpperInvariant();

            // valida conceptos permitidos
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { "I_MERCANCIAS", "I_SERVICIOS", "I_ANTICIPO" };

            if (!allowed.Contains(conceptoSerie))
                throw new InvalidOperationException($"conceptoSerie '{conceptoSerie}' no es válido.");

            // CP desde sucursal
            var sucursal = await _context.Sucursals.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == sucursalId && x.CuentaId == cuentaId && !x.IsDeleted, ct);

            if (sucursal is null)
                throw new InvalidOperationException("Sucursal no encontrada.");

            // serie/folio desde SucursalSerie
            var row = await _context.SucursalSeries.AsNoTracking()
                .Where(x => x.CuentaId == cuentaId
                    && x.SucursalId == sucursalId
                    && x.TipoCfdi == "I"
                    && x.Concepto == conceptoSerie
                    && x.Activo)
                .FirstOrDefaultAsync(ct);

            if (row is null || string.IsNullOrWhiteSpace(row.Serie))
                throw new InvalidOperationException(
                    $"No hay serie configurada para '{conceptoSerie}' en esta sucursal. Ve a Sucursales > Series y configúrala."
                );

            var nextFolio = row.FolioActual + 1;

            return new SerieFolioPreviewDto
            {
                Serie = row.Serie,
                Folio = nextFolio,
                ExpeditionPlace = sucursal.CodigoPostal // ✅ CP de expedición desde sucursal
            };
        }
        public async Task<Guid> CreateSucursalAsync(
    Guid cuentaId,
    string usuarioId,
    SucursalCreateDto dto,
    CancellationToken ct)
        {
            // =========================
            // Validaciones mínimas
            // =========================
            if (dto.RazonesSociales is null || dto.RazonesSociales.Count == 0)
                throw new InvalidOperationException("Selecciona al menos una razón social.");

            var now = DateTime.Now;

            // =========================
            // Normalizar + validar series (por Concepto)
            // =========================
            var incoming = new List<(string Concepto, char Tipo, string Serie, int? FolioInicial, bool Activo)>();

            foreach (var x in (dto.Series ?? new()))
            {
                var serie = (x.Serie ?? "").Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(serie))
                    continue; // si viene vacío, no lo mandamos

                var concepto = (x.Concepto ?? "").Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(concepto))
                    throw new InvalidOperationException("Serie inválida: falta Concepto.");

                var tipoStr = (x.TipoCfdi ?? "").Trim().ToUpperInvariant();
                var tipo = tipoStr.Length > 0 ? tipoStr[0] : '\0';

                if (tipo is not ('I' or 'E' or 'P'))
                    throw new InvalidOperationException($"Serie inválida: TipoCfdi '{x.TipoCfdi}' no es válido.");

                incoming.Add((concepto, tipo, serie, x.FolioInicial, x.Activo));
            }

            // ✅ Validación: 3 conceptos obligatorios
            static bool HasConcepto(List<(string Concepto, char Tipo, string Serie, int? FolioInicial, bool Activo)> list, string concepto) =>
                list.Any(s => string.Equals(s.Concepto, concepto, StringComparison.OrdinalIgnoreCase)
                           && !string.IsNullOrWhiteSpace(s.Serie));

            if (!HasConcepto(incoming, "I_MERCANCIAS") ||
                !HasConcepto(incoming, "I_SERVICIOS") ||
                !HasConcepto(incoming, "I_ANTICIPO"))
            {
                throw new InvalidOperationException("Las series de Tipo I (Mercancías/Servicios/Anticipo) son obligatorias.");
            }

            // ✅ No permitir duplicados por Concepto en el request
            var dup = incoming
                .GroupBy(x => x.Concepto, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (dup is not null)
                throw new InvalidOperationException($"Serie duplicada para el concepto '{dup.Key}'.");

            // =========================
            // Transacción
            // =========================
            using var tx = await _context.Database.BeginTransactionAsync(ct);

            // =========================
            // Crear sucursal
            // =========================
            var sucursal = new Sucursal
            {
                Id = Guid.NewGuid(),
                CuentaId = cuentaId,
                Codigo = (dto.Codigo ?? "").Trim().ToUpperInvariant(),
                Nombre = (dto.Nombre ?? "").Trim(),
                Activo = dto.Activo,

                Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),

                CodigoPostal = dto.Direccion?.CodigoPostal?.Trim(),
                Municipio = dto.Direccion?.Municipio?.Trim(),
                Estado = dto.Direccion?.Estado?.Trim(),
                Colonia = dto.Direccion?.Colonia?.Trim(),
                Calle = dto.Direccion?.Calle?.Trim(),
                NoInterior = dto.Direccion?.NoInterior?.Trim(),
                NoExterior = dto.Direccion?.NoExterior?.Trim(),

                IsDeleted = false,
                FechaCreacion = now,
                UsuarioCreacionId = usuarioId
            };

            _context.Sucursals.Add(sucursal);

            // =========================
            // Relaciones con razones sociales
            // =========================
            foreach (var rs in dto.RazonesSociales)
            {
                _context.SucursalRazonSocials.Add(new SucursalRazonSocial
                {
                    Id = Guid.NewGuid(),
                    CuentaId = cuentaId,
                    SucursalId = sucursal.Id,
                    RazonSocialId = rs.RazonSocialId,
                    Activo = rs.Activo,
                    EsDefault = rs.EsDefault,
                    FechaCreacion = now,
                    UsuarioCreacionId = usuarioId
                });
            }

            // =========================
            // Insertar series por concepto
            // =========================
            foreach (var s in incoming)
            {
                // FolioActual = FolioInicial - 1  (para que el siguiente sea FolioInicial)
                var folioActual = (s.FolioInicial.HasValue && s.FolioInicial.Value > 0)
                    ? s.FolioInicial.Value - 1
                    : 0;

                _context.SucursalSeries.Add(new SucursalSerie
                {
                    Id = Guid.NewGuid(),
                    CuentaId = cuentaId,
                    SucursalId = sucursal.Id,

                    Concepto = s.Concepto,
                    TipoCfdi = s.Tipo.ToString(),

                    Serie = s.Serie,
                    FolioActual = folioActual,
                    Activo = s.Activo,

                    FechaCreacion = now,
                    UsuarioCreacionId = usuarioId
                });
            }

            // =========================
            // Guardar
            // =========================
            try
            {
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return sucursal.Id;
            }
            catch (Exception ex) { throw; }
        }

        public async Task UpdateSucursalAsync(
     Guid cuentaId,
     string usuarioId,
     Guid sucursalId,
     SucursalUpdateDto dto,
     CancellationToken ct)
        {
            var sucursal = await _context.Sucursals
                .FirstOrDefaultAsync(x => x.Id == sucursalId && x.CuentaId == cuentaId && !x.IsDeleted, ct);

            if (sucursal is null)
                throw new InvalidOperationException("Sucursal no encontrada.");

            // Concurrencia por RowVersion
            var currentRv = Convert.ToBase64String(sucursal.RowVersion);
            if (!string.Equals(currentRv, dto.RowVersion, StringComparison.Ordinal))
                throw new InvalidOperationException("La sucursal fue modificada por otro usuario. Recarga e intenta de nuevo.");

            var now = DateTime.Now;

            using var tx = await _context.Database.BeginTransactionAsync(ct);

            // =========================
            // Update sucursal
            // =========================
            sucursal.Nombre = (dto.Nombre ?? "").Trim();
            sucursal.Activo = dto.Activo;

            sucursal.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
            sucursal.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();

            sucursal.CodigoPostal = dto.CodigoPostal?.Trim();
            sucursal.Municipio = dto.Municipio?.Trim();
            sucursal.Estado = dto.Estado?.Trim();
            sucursal.Colonia = dto.Colonia?.Trim();
            sucursal.Calle = dto.Calle?.Trim();
            sucursal.NoInterior = dto.NoInterior?.Trim();
            sucursal.NoExterior = dto.NoExterior?.Trim();

            sucursal.FechaModificacion = now;
            sucursal.UsuarioModificacionId = usuarioId;

            // =========================
            // Razones sociales (estrategia simple: desactivar y reactivar)
            // =========================
            var rels = await _context.SucursalRazonSocials
                .Where(x => x.CuentaId == cuentaId && x.SucursalId == sucursalId)
                .ToListAsync(ct);

            foreach (var rel in rels)
            {
                rel.Activo = false;
                rel.EsDefault = false;
                //rel.FechaModificacion = now;
                //rel.UsuarioModificacionId = usuarioId;
            }

            foreach (var rs in (dto.RazonesSociales ?? new()))
            {
                var rel = rels.FirstOrDefault(x => x.RazonSocialId == rs.RazonSocialId);
                if (rel is null)
                {
                    rel = new SucursalRazonSocial
                    {
                        Id = Guid.NewGuid(),
                        CuentaId = cuentaId,
                        SucursalId = sucursalId,
                        RazonSocialId = rs.RazonSocialId,
                        FechaCreacion = now,
                        UsuarioCreacionId = usuarioId
                    };
                    _context.SucursalRazonSocials.Add(rel);
                    rels.Add(rel);
                }

                rel.Activo = rs.Activo;
                rel.EsDefault = rs.EsDefault;
                //rel.FechaModificacion = now;
                //rel.UsuarioModificacionId = usuarioId;
            }

            // =========================
            // Series (upsert por Concepto)
            // Requiere: SucursalSerie.Concepto (varchar) + unique (SucursalId, Concepto)
            // =========================
            var existingSeries = await _context.SucursalSeries
                .Where(x => x.CuentaId == cuentaId && x.SucursalId == sucursalId)
                .ToListAsync(ct);

            // Normalizar incoming (solo las que traen serie)
            var incoming = (dto.Series ?? new())
                .Where(x => !string.IsNullOrWhiteSpace(x.Serie))
                .Select(x =>
                {
                    var concepto = (x.Concepto ?? "").Trim().ToUpperInvariant();
                    if (string.IsNullOrWhiteSpace(concepto))
                        throw new InvalidOperationException("Serie inválida: falta Concepto.");

                    var tipoStr = (x.TipoCfdi ?? "").Trim().ToUpperInvariant();
                    var tipo = (tipoStr.Length > 0) ? tipoStr[0] : '\0';
                    if (tipo is not ('I' or 'E' or 'P'))
                        throw new InvalidOperationException($"Serie inválida: TipoCfdi '{x.TipoCfdi}' no es válido.");

                    return new
                    {
                        Concepto = concepto,                              // ✅ clave
                        Tipo = tipo,                                      // I/E/P
                        Serie = x.Serie.Trim().ToUpperInvariant(),
                        FolioInicial = x.FolioInicial,
                        Activo = x.Activo
                    };
                })
                .ToList();

            // Validación: las 3 de tipo I por concepto deben venir con serie
            bool HasConcepto(string concepto) =>
                incoming.Any(x => string.Equals(x.Concepto, concepto, StringComparison.OrdinalIgnoreCase)
                               && !string.IsNullOrWhiteSpace(x.Serie));

            if (!HasConcepto("I_MERCANCIAS") || !HasConcepto("I_SERVICIOS") || !HasConcepto("I_ANTICIPO"))
                throw new InvalidOperationException("Las series de Tipo I (Mercancías/Servicios/Anticipo) son obligatorias.");

            // Diccionario existentes por concepto
            var existingByConcepto = existingSeries
                .Where(x => !string.IsNullOrWhiteSpace(x.Concepto))
                .ToDictionary(x => x.Concepto.Trim().ToUpperInvariant(), x => x);

            // Upsert
            foreach (var inc in incoming)
            {
                existingByConcepto.TryGetValue(inc.Concepto, out var row);

                if (row is null)
                {
                    var folioActual = (inc.FolioInicial.HasValue && inc.FolioInicial.Value > 0)
                        ? inc.FolioInicial.Value - 1
                        : 0;

                    row = new SucursalSerie
                    {
                        Id = Guid.NewGuid(),
                        CuentaId = cuentaId,
                        SucursalId = sucursalId,

                        Concepto = inc.Concepto,          // ✅ NUEVO
                        TipoCfdi = inc.Tipo.ToString(),   // I/E/P (informativo)

                        Serie = inc.Serie,
                        FolioActual = folioActual,
                        Activo = inc.Activo,

                        FechaCreacion = now,
                        UsuarioCreacionId = usuarioId
                    };

                    _context.SucursalSeries.Add(row);
                    existingSeries.Add(row);
                    existingByConcepto[inc.Concepto] = row;
                }
                else
                {
                    // No tocar FolioActual aquí (solo al timbrar).
                    row.TipoCfdi = inc.Tipo.ToString();
                    row.Serie = inc.Serie;
                    row.Activo = inc.Activo;

                    row.FechaModificacion = now;
                    row.UsuarioModificacionId = usuarioId;
                }
            }

            // (Opcional) Desactivar las series existentes que no vengan en el request
            // Nota: si tu UI siempre manda las 7, puedes usar esto para "sincronizar".
            // Si NO quieres desactivar nunca, comenta este bloque.
            var incomingConceptos = incoming
                .Select(x => x.Concepto)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var ex in existingSeries)
            {
                if (string.IsNullOrWhiteSpace(ex.Concepto)) continue;

                if (!incomingConceptos.Contains(ex.Concepto))
                {
                    ex.Activo = false;
                    ex.FechaModificacion = now;
                    ex.UsuarioModificacionId = usuarioId;
                }
            }

            try
            {
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                // aquí puedes loggear si quieres
                throw;
            }
        }
    }
}
