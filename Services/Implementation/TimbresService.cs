using Facturacion.API.Data;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto.Timbres;
using Facturacion.API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace Facturacion.API.Services.Implementation
{
    public class TimbresService : ITimbresService
    {
        private readonly FacturacionContext _context;

        public TimbresService(FacturacionContext context)
        {
            _context = context;
        }

        public async Task AsegurarSaldoCuentaAsync(Guid cuentaId, CancellationToken ct)
        {
            var exists = await _context.CuentaTimbres.AnyAsync(x => x.CuentaId == cuentaId, ct);
            if (exists) return;

            _context.CuentaTimbres.Add(new CuentaTimbre
            {
                CuentaId = cuentaId,
                Disponibles = 0,
                Consumidos = 0,
                UpdatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);
        }

        public async Task ValidarDisponiblesAsync(Guid cuentaId, CancellationToken ct)
        {
            await AsegurarSaldoCuentaAsync(cuentaId, ct);

            var saldo = await _context.CuentaTimbres
                .AsNoTracking()
                .FirstAsync(x => x.CuentaId == cuentaId, ct);

            if (saldo.Disponibles <= 0)
                throw new InvalidOperationException("No hay timbres disponibles para esta cuenta.");
        }

        public async Task RegistrarConsumoTimbradoAsync(
            Guid cuentaId,
            Guid? cfdiId,
            string? facturamaId,
            string? uuid,
            string accion,
            string? createdBy,
            CancellationToken ct)
        {
            await AsegurarSaldoCuentaAsync(cuentaId, ct);

            // Idempotencia por UUID cuando exista
            if (!string.IsNullOrWhiteSpace(uuid))
            {
                var ya = await _context.CuentaTimbreMovimientos
                    .AsNoTracking()
                    .AnyAsync(x => x.CuentaId == cuentaId && x.Uuid == uuid &&
                                   (x.Accion == TimbreAcciones.Timbrado || x.Accion == TimbreAcciones.NotaCredito || x.Accion == TimbreAcciones.TimbradoPago),
                        ct);

                if (ya) return; // ya descontado
            }

            using var tx = await _context.Database.BeginTransactionAsync(ct);

            // Bloqueo pesimista: asegura que dos timbrados simultáneos no pasen con 1 timbre
            // (SQL Server) FOR UPDATE equivalente con UPDLOCK/HOLDLOCK vía raw SQL:
            var saldo = await _context.CuentaTimbres
                .FromSqlInterpolated($@"
                SELECT * FROM dbo.CuentaTimbres WITH (UPDLOCK, HOLDLOCK)
                WHERE CuentaId = {cuentaId}")
                .SingleAsync(ct);

            if (saldo.Disponibles <= 0)
                throw new InvalidOperationException("No hay timbres disponibles para esta cuenta.");

            saldo.Disponibles -= 1;
            saldo.Consumidos += 1;
            saldo.UpdatedAt = DateTime.UtcNow;

            _context.CuentaTimbreMovimientos.Add(new CuentaTimbreMovimiento
            {
                Id = Guid.NewGuid(),
                CuentaId = cuentaId,
                Accion = accion,
                Timbres = -1,
                CfdiId = cfdiId,
                FacturamaId = facturamaId,
                Uuid = uuid,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            });

            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        public async Task RegistrarCompraOAjusteAsync(Guid cuentaId, int timbres, string accion, string? referencia, string? notas, string? createdBy, CancellationToken ct)
        {
            if (timbres == 0) return;

            await AsegurarSaldoCuentaAsync(cuentaId, ct);

            using var tx = await _context.Database.BeginTransactionAsync(ct);

            var saldo = await _context.CuentaTimbres
                .FromSqlInterpolated($@"
                SELECT * FROM dbo.CuentaTimbres WITH (UPDLOCK, HOLDLOCK)
                WHERE CuentaId = {cuentaId}")
                .SingleAsync(ct);

            saldo.Disponibles += timbres; // compra +, ajuste +/- según tu uso
            saldo.UpdatedAt = DateTime.UtcNow;

            _context.CuentaTimbreMovimientos.Add(new CuentaTimbreMovimiento
            {
                Id = Guid.NewGuid(),
                CuentaId = cuentaId,
                Accion = accion,
                Timbres = timbres,
                Referencia = referencia,
                Notas = notas,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            });

            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        public async Task<(int disponibles, int consumidos)> GetResumenAsync(Guid cuentaId, CancellationToken ct)
        {
            await AsegurarSaldoCuentaAsync(cuentaId, ct);

            var saldo = await _context.CuentaTimbres
                .AsNoTracking()
                .FirstAsync(x => x.CuentaId == cuentaId, ct);

            return (saldo.Disponibles, saldo.Consumidos);
        }
    }
}
