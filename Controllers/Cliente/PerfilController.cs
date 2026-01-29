using AutoMapper;
using Facturacion.API.Data;
using Facturacion.API.Helpers;
using Facturacion.API.Models;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto;
using Facturacion.API.Models.Dto.Auth;
using Facturacion.API.Models.Dto.Cliente.Catalogos;
using Facturacion.API.Models.Dto.Cliente.Perfil;
using Facturacion.API.Models.Perfil;
using Facturacion.API.Models.Specifications;
using Facturacion.API.Repositories.Interface;
using Facturacion.API.Services.Implementation;
using Facturacion.API.Services.Interface;
using iText.Kernel.Pdf.Canvas.Wmf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Facturacion.API.Controllers.Cliente
{
    [Route("api/cliente/[controller]")]
    [ApiController]
    public class PerfilController : ControllerBase
    {
        private readonly IGenericRepository<RazonSocial> _repoRazonSocial;
        private readonly IMapper _mapper;
        private readonly IConfiguration config;
        private readonly IPerfilService perfilService;
        private readonly ICryptoService _cryptoService;
        private readonly FacturacionContext _context;

        public PerfilController(
            IMapper mapper,
            IGenericRepository<RazonSocial> _repoRazonSocial,
            IConfiguration config,
            IPerfilService perfilService,
            ICryptoService cryptoService,
            FacturacionContext context
            )
        {
            this._repoRazonSocial = _repoRazonSocial;
            this.config = config;
            this.perfilService = perfilService;
            _cryptoService = cryptoService;
            _mapper = mapper;
            _context = context;
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost("CrearRazonSocial")]
        public async Task<IActionResult> CrearRazonSocial([FromBody] CrearRazonSocialRequest model)
        {
            var permiteMultiRazonSocial =
            int.Parse(config["Configuracion:MultiRazonesSociales"]);
            var cuentaId = User.GetCuentaId();

            // Si NO permite múltiples razones sociales
            if (permiteMultiRazonSocial == 0)
            {
                var filtro = new FiltroGlobal
                {
                    IncluirInactivos = false
                };

                var pagination = new PaginationFilter
                {
                    PageNumber = 1,
                    PageSize = 1 // solo necesitamos saber si existe
                };

                var spec = new RazonSocialSpecification(filtro, Guid.Parse(cuentaId));

                var result = await _repoRazonSocial.ListAsync(spec, pagination);

                if (result.TotalItems > 0)
                {
                    //actualizamos la nueva
                    var res = result.Items.FirstOrDefault();
                    res.Rfc = model.rfc;
                    res.RazonSocial1 = model.razonSocial;
                    res.RegimenFiscalId = model.regimenFiscalId;
                    res.CelularNotificaciones = model.celularNotificaciones;
                    res.FolioInicio = model.folioInicio;
                    res.CodigoPostal = model.codigoPostal;
                    res.Municipio = model.municipio;
                    res.Estado = model.estado;
                    res.Colonia = model.colonia;
                    res.Calle = model.calle;
                    res.NoInterior = model.noInterior;
                    res.NoExterior = model.noExterior;

                    await this._repoRazonSocial.UpdateAsync(res);
                    await this._repoRazonSocial.SaveChangesAsync();
                    return Ok();
                }

                //guardamos el perfil
                RazonSocial razonSocial = new RazonSocial()
                {
                    Id = Guid.NewGuid(),
                    CuentaId = Guid.Parse(cuentaId),
                    Rfc = model.rfc,
                    RazonSocial1 = model.razonSocial,
                    RegimenFiscalId = model.regimenFiscalId,
                    CelularNotificaciones = model.celularNotificaciones,
                    FolioInicio = model.folioInicio,
                    CodigoPostal = model.codigoPostal,
                    Municipio = model.municipio,
                    Estado = model.estado,  
                    Colonia = model.colonia,    
                    Calle = model.calle,
                    NoInterior = model.noInterior,
                    NoExterior = model.noExterior,
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacionId = User.GetId(),
                };

                await this._repoRazonSocial.AddAsync(razonSocial);
                await this._repoRazonSocial.SaveChangesAsync();
            }
            return Ok();
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("GetRazonSocial")]
        public async Task<IActionResult> GetRazonSocial()
        {
            var cuentaId = User.GetCuentaId();

            var userid = User.GetId();

            var filtro = new FiltroGlobal
            {
                IncluirInactivos = false
            };

            var spec = new RazonSocialSpecification(filtro, Guid.Parse(cuentaId));


            var resultList = await _repoRazonSocial.ListAsync(spec);
            var result = resultList.FirstOrDefault();
            var dto = _mapper.Map<GetRazonSocialResponse>(result);
            return Ok(dto);
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost("EnviarSellosDigitales")]
        public async Task<IActionResult> EnviarSellosDigitales(
        [FromBody] EnviarSellosDigitalesRequest request)
        {
            return Ok();
            /*
            if (string.IsNullOrEmpty(request.CertificadoBase64) ||
                string.IsNullOrEmpty(request.LlavePrivadaBase64) ||
                string.IsNullOrEmpty(request.PasswordLlave))
            {
                return BadRequest("Certificado, llave y contraseña son obligatorios.");
            }

            var cuentaId = User.GetCuentaId(); // desde JWT

            await this.perfilService.GuardarSellosAsync(Guid.Parse(cuentaId), request);

            return Ok(new
            {
                message = "Sellos digitales guardados correctamente"
            });
            */
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost("SubirSellosAFacturama")]
        public async Task<IActionResult> SubirSellosAFacturama([FromQuery] bool force = false, CancellationToken ct = default)
        {
            /*
            var cuentaId = Guid.Parse(User.GetCuentaId());
            force = true;
            await perfilService.SubirSellosAFacturamaAsync(cuentaId, force, ct);

            return Ok(new { message = "Sellos sincronizados con Facturama correctamente" });
            */

            return Ok();
        }

        [HttpGet("razones-sociales/{razonSocialId:guid}/sellos/descargar")]
        public async Task<IActionResult> DescargarSellos(
    Guid razonSocialId,
    CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());

            // ✅ traer la razón social exacta
            var razon = await _context.RazonSocials.AsNoTracking().FirstOrDefaultAsync(x => x.Id == razonSocialId && x.CuentaId == cuentaId, ct);
                //.FirstOrDefaultAsync(x => x.Id == razonSocialId && x.CuentaId == cuentaId, ct);

            if (razon is null)
                return NotFound("No existe la razón social.");

            if (razon.CerBase64 is null || razon.KeyBase64 is null || razon.KeyPassword is null)
                return NotFound("No hay sellos digitales registrados para esta razón social.");

            // 🔓 Desencriptar (si falla, error claro)
            string cerBase64;
            string keyBase64;

            try
            {
                cerBase64 = _cryptoService.Decrypt(razon.CerBase64);
                keyBase64 = _cryptoService.Decrypt(razon.KeyBase64);
            }
            catch
            {
                return BadRequest("No se pudieron desencriptar los sellos. Vuelve a cargarlos.");
            }

            // Convertir a bytes reales (si falla base64, error claro)
            byte[] cerBytes;
            byte[] keyBytes;

            try
            {
                cerBytes = Convert.FromBase64String(cerBase64);
                keyBytes = Convert.FromBase64String(keyBase64);
            }
            catch
            {
                return BadRequest("Los sellos guardados no tienen un formato Base64 válido.");
            }

            // 📦 Crear ZIP en memoria
            await using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                var rfc = (razon.Rfc ?? "EMISOR").Trim().ToUpperInvariant();

                var cerEntry = zip.CreateEntry($"{rfc}.cer");
                await using (var cerStream = cerEntry.Open())
                    await cerStream.WriteAsync(cerBytes, 0, cerBytes.Length, ct);

                var keyEntry = zip.CreateEntry($"{rfc}.key");
                await using (var keyStream = keyEntry.Open())
                    await keyStream.WriteAsync(keyBytes, 0, keyBytes.Length, ct);

                // (Opcional) metadata sin password
                var infoEntry = zip.CreateEntry("info.txt");
                await using (var infoStream = new StreamWriter(infoEntry.Open()))
                {
                    await infoStream.WriteLineAsync($"RFC: {rfc}");
                    await infoStream.WriteLineAsync($"Razon social: {razon.RazonSocial1}");
                    await infoStream.WriteLineAsync($"Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                }
            }

            ms.Position = 0;

            var fileName = $"sellos_{(razon.Rfc ?? "emisor").Trim().ToUpperInvariant()}.zip";

            return File(
                ms.ToArray(),
                "application/zip",
                fileName
            );
        }

        //------------------
        [HttpGet("razones-sociales")]
        public async Task<IActionResult> GetRazonesSociales(CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId()); // helper desde claims

            var data = await perfilService.GetRazonesSocialesAsync(cuentaId, ct);

            return Ok(data);
        }

        [HttpPost("razones-sociales")]
        public async Task<IActionResult> UpsertRazonSocial(
        [FromBody] UpsertRazonSocialRequest request,
        CancellationToken ct)
        {
            if (request == null)
                return BadRequest("Payload inválido");

            var cuentaId = Guid.Parse(User.GetCuentaId());

            var razonSocialId = await perfilService.UpsertRazonSocialAsync(
                cuentaId,
                User.GetId(),
                request,
                ct
            );

            return Ok(new
            {
                id = razonSocialId
            });
        }

        [HttpPost("razones-sociales/{razonSocialId:guid}/default")]
        public async Task<IActionResult> SetDefaultRazonSocial(
        Guid razonSocialId,
        CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());

            await perfilService.SetDefaultRazonSocialAsync(cuentaId, razonSocialId, ct);

            return Ok(new { ok = true });
        }

        [HttpDelete("razones-sociales/{razonSocialId:guid}")]
        public async Task<IActionResult> DeleteRazonSocial(
        Guid razonSocialId,
        CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());

            await perfilService.DeleteRazonSocialAsync(cuentaId, razonSocialId, ct);

            return Ok(new { ok = true });
        }

        [HttpPost("razones-sociales/{razonSocialId:guid}/sellos")]
        public async Task<IActionResult> GuardarSellos(
    Guid razonSocialId,
    [FromBody] EnviarSellosDigitalesRequest request,
    CancellationToken ct)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());
            await perfilService.GuardarSellosAsync(cuentaId, razonSocialId, request, ct);
            return Ok(new { ok = true });
        }

        [HttpPost("razones-sociales/{razonSocialId:guid}/sellos/subir-facturama")]
        public async Task<IActionResult> SubirSellosAFacturama(
    Guid razonSocialId,
    [FromQuery] bool force = false,
    CancellationToken ct = default)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());
            force = true;
            await perfilService.SubirSellosAFacturamaAsync(cuentaId, razonSocialId, force, ct);
            return Ok(new { ok = true });
        }
    }
}

