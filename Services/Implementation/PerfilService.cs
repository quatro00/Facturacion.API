using Facturacion.API.Data;
using Facturacion.API.Models.Dto.Cliente.Factura;
using Facturacion.API.Models.Dto.Cliente.Perfil;
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
    }
}
