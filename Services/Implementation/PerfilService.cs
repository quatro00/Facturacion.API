using Facturacion.API.Data;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto.Cliente.Factura;
using Facturacion.API.Models.Dto.Cliente.Perfil;
using Facturacion.API.Models.Perfil;
using Facturacion.API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Facturacion.API.Services.Implementation
{
    public class PerfilService : IPerfilService
    {
        private readonly IConfiguration _config;
        private readonly FacturacionContext _context;
        private readonly ICryptoService _cryptoService;
        private readonly IFacturamaClient _facturamaClient;

        public PerfilService(
        IConfiguration config,
        FacturacionContext context,
        ICryptoService cryptoService,
        IFacturamaClient facturamaClient
            )
        {
            _config = config;
            _context = context;
            _cryptoService = cryptoService;
            _facturamaClient = facturamaClient;
        }

        public async Task DeleteRazonSocialAsync(
    Guid cuentaId,
    Guid razonSocialId,
    CancellationToken ct = default)
        {
            var razonSocial = await _context.RazonSocials
                .FirstOrDefaultAsync(x =>
                    x.Id == razonSocialId &&
                    x.CuentaId == cuentaId,
                    ct);

            if (razonSocial == null)
                throw new KeyNotFoundException("Razón social no encontrada.");

            // 1️⃣ No permitir borrar la default
            if (razonSocial.EsDefault)
                throw new InvalidOperationException(
                    "No puedes eliminar la razón social predeterminada."
                );

            // 2️⃣ No permitir borrar si ya tiene CFDIs emitidos
            // (ajusta el DbSet si se llama distinto)
            var rfc = razonSocial.Rfc.Trim().ToUpperInvariant();

            var tieneCfdis = await _context.Cfdis
                .AsNoTracking()
                .AnyAsync(x =>
                    x.CuentaId == cuentaId &&
                    x.RfcEmisor == rfc,
                    ct);

            if (tieneCfdis)
                throw new InvalidOperationException(
                    "No puedes eliminar una razón social que ya tiene CFDIs emitidos."
                );

            // 3️⃣ (Opcional) Limpieza de sellos antes de borrar
            // Si guardas sellos en la misma tabla, no hace falta nada extra
            // Si los guardas en otra tabla, aquí deberías borrarlos

            _context.RazonSocials.Remove(razonSocial);

            await _context.SaveChangesAsync(ct);
        }

        public Task<Stream> DescargarSellosAsync(Guid cuentaId, Guid razonSocialId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<RazonSocialDto>> GetRazonesSocialesAsync(
    Guid cuentaId,
    CancellationToken ct = default)
        {
            // Ajusta el DbSet según tu DbContext:
            // _context.RazonSocials o _context.RazonSocial
            var data = await _context.RazonSocials
                .AsNoTracking()
                .Where(x => x.CuentaId == cuentaId)
                // orden recomendado: primero default, luego activos, luego por nombre
                .OrderByDescending(x => x.EsDefault)
                .ThenByDescending(x => x.Activo)
                .ThenBy(x => x.RazonSocial1)
                .Select(x => new RazonSocialDto
                {
                    Id = x.Id,
                    CuentaId = x.CuentaId,

                    Rfc = x.Rfc,
                    RazonSocial = x.RazonSocial1,
                    NombreComercial = x.NombreComercial,

                    RegimenFiscalId = x.RegimenFiscalId,
                    RegimenFiscalCodigo = x.RegimenFiscal.RegimenFiscal,
                    RegimenFiscal = x.RegimenFiscal.Descripcion,

                    EmailEmisor = x.EmailEmisor,
                    TelefonoEmisor = x.TelefonoEmisor,
                    CelularNotificaciones = x.CelularNotificaciones,

                    FolioInicio = x.FolioInicio,

                    SerieIngresoDefault = x.SerieIngresoDefault,
                    SerieEgresoDefault = x.SerieEgresoDefault,
                    CpLugarExpedicionDefault = x.CpLugarExpedicionDefault,

                    CodigoPostal = x.CodigoPostal,
                    Estado = x.Estado,
                    Municipio = x.Municipio,
                    Colonia = x.Colonia,
                    Calle = x.Calle,
                    NoExterior = x.NoExterior,
                    NoInterior = x.NoInterior,

                    Activo = x.Activo,
                    EsDefault = x.EsDefault,

                    FacturamaIssuerId = x.FacturamaIssuerId,

                    // útil para UI: si ya tiene los 3 insumos
                    TieneSellos = x.CerBase64 != null && x.KeyBase64 != null && x.KeyPassword != null,

                    FechaCreacion = x.FechaCreacion
                })
                .ToListAsync(ct);

            return data;
        }

        public async Task GuardarSellosAsync(
        Guid cuentaId,
        EnviarSellosDigitalesRequest request)
        {
            var encryptedCer = _cryptoService.Encrypt(request.CertificadoBase64);
            var encryptedKey = _cryptoService.Encrypt(request.LlavePrivadaBase64);
            var encryptedPassword = _cryptoService.Encrypt(request.PasswordLlave);

            var razonSocial = await this._context.RazonSocials.Where(x => x.CuentaId == cuentaId).FirstOrDefaultAsync();

            razonSocial.CerBase64 = encryptedCer;
            razonSocial.KeyBase64 = encryptedKey;
            razonSocial.KeyPassword = encryptedPassword;

            await _context.SaveChangesAsync();
        }

        public async Task GuardarSellosAsync(
    Guid cuentaId,
    Guid razonSocialId,
    EnviarSellosDigitalesRequest request,
    CancellationToken ct = default)
        {
            var razonSocial = await _context.RazonSocials
                .FirstOrDefaultAsync(x => x.Id == razonSocialId && x.CuentaId == cuentaId, ct);

            if (razonSocial is null)
                throw new InvalidOperationException("Razón social no encontrada.");

            // Validación base64
            _ = Convert.FromBase64String(request.CertificadoBase64);
            _ = Convert.FromBase64String(request.LlavePrivadaBase64);

            razonSocial.CerBase64 = _cryptoService.Encrypt(request.CertificadoBase64);
            razonSocial.KeyBase64 = _cryptoService.Encrypt(request.LlavePrivadaBase64);
            razonSocial.KeyPassword = _cryptoService.Encrypt(request.PasswordLlave);

            razonSocial.FechaModificacion = DateTime.UtcNow;
            razonSocial.UsuarioModificacionId = "system";

            await _context.SaveChangesAsync(ct);
        }

        public async Task SetDefaultRazonSocialAsync(
    Guid cuentaId,
    Guid razonSocialId,
    CancellationToken ct = default)
        {
            // Trae la razón social y valida que sea de la cuenta
            var target = await _context.RazonSocials
                .FirstOrDefaultAsync(x => x.Id == razonSocialId && x.CuentaId == cuentaId, ct);

            if (target == null)
                throw new KeyNotFoundException("Razón social no encontrada.");

            // Si ya es default, no hagas nada
            if (target.EsDefault)
                return;

            // Transacción para cumplir el índice único filtrado (solo 1 EsDefault=1 por cuenta)
            await using var tx = await _context.Database.BeginTransactionAsync(ct);

            // Quitar default actual
            var actualesDefault = await _context.RazonSocials
                .Where(x => x.CuentaId == cuentaId && x.EsDefault)
                .ToListAsync(ct);

            foreach (var item in actualesDefault)
            {
                item.EsDefault = false;
            }

            // Poner default al target
            target.EsDefault = true;
            target.Activo = true; // regla: default siempre activo

            target.FechaModificacion = DateTime.UtcNow;
            target.UsuarioModificacionId = "system"; // o del token

            try
            {
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch (Exception ex) { 
            int y=0;
            }

            

            
        }

        public async Task SubirSellosAFacturamaAsync(Guid cuentaId, bool force = false, CancellationToken ct = default)
        {
            var razon = await _context.RazonSocials
        .FirstOrDefaultAsync(x => x.CuentaId == cuentaId, ct);

            if (razon is null)
                throw new InvalidOperationException("No existe emisor asociado a la cuenta.");

            if (razon.CerBase64 is null || razon.KeyBase64 is null || razon.KeyPassword is null)
                throw new InvalidOperationException("No hay sellos guardados para esta cuenta.");

            string cerBase64 = _cryptoService.Decrypt(razon.CerBase64);
            string keyBase64 = _cryptoService.Decrypt(razon.KeyBase64);
            string pwd = _cryptoService.Decrypt(razon.KeyPassword);

            // Validación Base64
            _ = Convert.FromBase64String(cerBase64);
            _ = Convert.FromBase64String(keyBase64);

            var payload = new FacturamaCsdRequest
            {
                Rfc = razon.Rfc.Trim().ToUpperInvariant(),
                Certificate = cerBase64,
                PrivateKey = keyBase64,
                PrivateKeyPassword = pwd
            };

            var existe = await _facturamaClient.ExisteCsdAsync(payload.Rfc, ct);

            if (existe && !force)
                throw new InvalidOperationException("El RFC ya tiene CSD en Facturama. Usa force=true para actualizar.");

            if (!existe)
                await _facturamaClient.CrearCsdAsync(payload, ct);
            else
                await _facturamaClient.ActualizarCsdAsync(payload.Rfc, payload, ct);

            // 5) Marcar estado local (recomendado)
            //emisor.SellosCargadosFacturama = true;
            //emisor.FechaUltimaCargaSellosFacturama = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SubirSellosAFacturamaAsync(
    Guid cuentaId,
    Guid razonSocialId,
    bool force = false,
    CancellationToken ct = default)
        {
            var razon = await _context.RazonSocials
                .FirstOrDefaultAsync(x => x.Id == razonSocialId && x.CuentaId == cuentaId, ct);

            if (razon is null)
                throw new InvalidOperationException("Razón social no encontrada.");

            if (razon.CerBase64 is null || razon.KeyBase64 is null || razon.KeyPassword is null)
                throw new InvalidOperationException("No hay sellos guardados para esta razón social.");

            string cerBase64 = _cryptoService.Decrypt(razon.CerBase64);
            string keyBase64 = _cryptoService.Decrypt(razon.KeyBase64);
            string pwd = _cryptoService.Decrypt(razon.KeyPassword);

            _ = Convert.FromBase64String(cerBase64);
            _ = Convert.FromBase64String(keyBase64);

            var payload = new FacturamaCsdRequest
            {
                Rfc = razon.Rfc.Trim().ToUpperInvariant(),
                Certificate = cerBase64,
                PrivateKey = keyBase64,
                PrivateKeyPassword = pwd
            };

            var existe = await _facturamaClient.ExisteCsdAsync(payload.Rfc, ct);

            if (existe && !force)
                throw new InvalidOperationException("El RFC ya tiene CSD en Facturama. Usa force=true para actualizar.");

            if (!existe)
                await _facturamaClient.CrearCsdAsync(payload, ct);
            else
                await _facturamaClient.ActualizarCsdAsync(payload.Rfc, payload, ct);

            razon.FechaModificacion = DateTime.UtcNow;
            razon.UsuarioModificacionId = "system";

            await _context.SaveChangesAsync(ct);
        }

        public async Task<Guid> UpsertRazonSocialAsync(
    Guid cuentaId,
    string usuarioId,
    UpsertRazonSocialRequest request,
    CancellationToken ct = default)
        {
            // Normalización básica
            var rfc = request.Rfc.Trim().ToUpperInvariant();

            // 1. Validar RFC único por cuenta
            var existeRfc = await _context.RazonSocials
                .AnyAsync(x =>
                    x.CuentaId == cuentaId &&
                    x.Rfc == rfc &&
                    (request.Id == null || x.Id != request.Id.Value),
                    ct);

            if (existeRfc)
                throw new InvalidOperationException("Ya existe una razón social con ese RFC.");

            // 2. CREATE
            if (request.Id == null || request.Id == Guid.Empty)
            {
                var esPrimera = !await _context.RazonSocials
                    .AnyAsync(x => x.CuentaId == cuentaId, ct);

                var entity = new RazonSocial
                {
                    Id = Guid.NewGuid(),
                    CuentaId = cuentaId,

                    Rfc = rfc,
                    RazonSocial1 = request.RazonSocial.Trim(),
                    RegimenFiscalId = request.RegimenFiscalId,

                    NombreComercial = request.NombreComercial?.Trim(),
                    EmailEmisor = request.EmailEmisor?.Trim(),
                    TelefonoEmisor = request.TelefonoEmisor?.Trim(),
                    CelularNotificaciones = request.CelularNotificaciones ?? "".Trim(),

                    FolioInicio = request.FolioInicio,

                    SerieIngresoDefault = request.SerieIngresoDefault.Trim(),
                    SerieEgresoDefault = request.SerieEgresoDefault?.Trim(),
                    CpLugarExpedicionDefault = request.CpLugarExpedicionDefault?.Trim(),

                    CodigoPostal = request.CodigoPostal.Trim(),
                    Estado = request.Estado.Trim(),
                    Municipio = request.Municipio.Trim(),
                    Colonia = request.Colonia.Trim(),
                    Calle = request.Calle.Trim(),
                    NoExterior = request.NoExterior.Trim(),
                    NoInterior = request.NoInterior?.Trim(),

                    Activo = true,
                    EsDefault = esPrimera, // la primera siempre es default

                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacionId = usuarioId // o desde token
                };

                _context.RazonSocials.Add(entity);
                //await _context.SaveChangesAsync(ct);

                try
                {
                    await _context.SaveChangesAsync(ct);
                }
                catch (Exception ex)
                {
                    int y = 0;
                }

                return entity.Id;
            }

            // 3. UPDATE
            var razonSocial = await _context.RazonSocials
                .FirstOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.CuentaId == cuentaId,
                    ct);

            if (razonSocial == null)
                throw new KeyNotFoundException("Razón social no encontrada.");

            // Regla: si es default no se puede desactivar
            if (razonSocial.EsDefault && request.Activa == false)
                throw new InvalidOperationException("No puedes desactivar la razón social predeterminada.");

            // (opcional pero recomendado)
            // Si ya tiene CFDIs, NO permitir cambiar RFC o Régimen
            /*
            var tieneCfdis = await _context.Cfdis
                .AnyAsync(x => x.RazonSocialId == razonSocial.Id, ct);

            if (tieneCfdis)
            {
                if (razonSocial.Rfc != rfc)
                    throw new InvalidOperationException("No puedes cambiar el RFC si ya existen CFDIs.");
                if (razonSocial.RegimenFiscalId != request.RegimenFiscalId)
                    throw new InvalidOperationException("No puedes cambiar el régimen fiscal si ya existen CFDIs.");
            }
            */

            // Actualizar campos
            razonSocial.Rfc = rfc;
            razonSocial.RazonSocial1 = request.RazonSocial.Trim();
            razonSocial.RegimenFiscalId = request.RegimenFiscalId;

            razonSocial.NombreComercial = request.NombreComercial?.Trim();
            razonSocial.EmailEmisor = request.EmailEmisor?.Trim();
            razonSocial.TelefonoEmisor = request.TelefonoEmisor?.Trim();
            razonSocial.CelularNotificaciones = request.CelularNotificaciones?? "".Trim();

            razonSocial.FolioInicio = request.FolioInicio;
            razonSocial.SerieIngresoDefault = request.SerieIngresoDefault.Trim();
            razonSocial.SerieEgresoDefault = request.SerieEgresoDefault?.Trim();
            razonSocial.CpLugarExpedicionDefault = request.CpLugarExpedicionDefault?.Trim();

            razonSocial.CodigoPostal = request.CodigoPostal.Trim();
            razonSocial.Estado = request.Estado.Trim();
            razonSocial.Municipio = request.Municipio.Trim();
            razonSocial.Colonia = request.Colonia.Trim();
            razonSocial.Calle = request.Calle.Trim();
            razonSocial.NoExterior = request.NoExterior.Trim();
            razonSocial.NoInterior = request.NoInterior?.Trim();

            razonSocial.Activo = request.Activa;

            razonSocial.FechaModificacion =  DateTime.Now;
            razonSocial.UsuarioModificacionId = usuarioId; // o del token
            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex) {
                int y = 0;
            }
            

            return razonSocial.Id;
        }
    }
}
