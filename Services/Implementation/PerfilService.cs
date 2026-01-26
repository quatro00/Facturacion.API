using Facturacion.API.Data;
using Facturacion.API.Models.Dto.Cliente.Perfil;
using Facturacion.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.API.Services.Implementation
{
    public class PerfilService : IPerfilService
    {
        private readonly IConfiguration _config;
        private readonly FacturacionContext _context;
        private readonly ICryptoService _cryptoService;

        public PerfilService(
        IConfiguration config,
        FacturacionContext context,
        ICryptoService cryptoService
            )
        {
            _config = config;
            _context = context;
            _cryptoService = cryptoService;
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
    }
}
