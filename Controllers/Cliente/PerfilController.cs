using AutoMapper;
using Facturacion.API.Helpers;
using Facturacion.API.Models;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto;
using Facturacion.API.Models.Dto.Auth;
using Facturacion.API.Models.Dto.Cliente.Catalogos;
using Facturacion.API.Models.Dto.Cliente.Perfil;
using Facturacion.API.Models.Specifications;
using Facturacion.API.Repositories.Interface;
using Facturacion.API.Services.Interface;
using iText.Kernel.Pdf.Canvas.Wmf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
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

        public PerfilController(
            IMapper mapper,
            IGenericRepository<RazonSocial> _repoRazonSocial,
            IConfiguration config,
            IPerfilService perfilService,
            ICryptoService cryptoService
            )
        {
            this._repoRazonSocial = _repoRazonSocial;
            this.config = config;
            this.perfilService = perfilService;
            _cryptoService = cryptoService;
            _mapper = mapper;
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
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost("SubirSellosAFacturama")]
        public async Task<IActionResult> SubirSellosAFacturama([FromQuery] bool force = false, CancellationToken ct = default)
        {
            var cuentaId = Guid.Parse(User.GetCuentaId());
            force = true;
            await perfilService.SubirSellosAFacturamaAsync(cuentaId, force, ct);

            return Ok(new { message = "Sellos sincronizados con Facturama correctamente" });
        }

        [HttpGet("DescargarSellos")]
        public async Task<IActionResult> DescargarSellos()
        {
            var cuentaId = User.GetCuentaId();
            var filtro = new FiltroGlobal
            {
                IncluirInactivos = false
            };
            var spec = new RazonSocialSpecification(filtro, Guid.Parse(cuentaId));


            var resultList = await _repoRazonSocial.ListAsync(spec);
            var sellos = resultList.FirstOrDefault();

            if (sellos == null)
            {
                return NotFound("No hay sellos digitales registrados.");
            }

            if (sellos.CerBase64 == null || sellos.KeyPassword == null || sellos.KeyBase64 == null)
            {
                return NotFound("No hay sellos digitales registrados.");
            }


            // 🔓 Desencriptar
            try {
                var cerBase64_2 = _cryptoService.Decrypt(sellos.CerBase64);
                var keyBase64_2 = _cryptoService.Decrypt(sellos.KeyBase64);
            }
            catch (Exception ex) { 
            
            }
            var cerBase64 = _cryptoService.Decrypt(sellos.CerBase64);
            var keyBase64 = _cryptoService.Decrypt(sellos.KeyBase64);

            // Convertir a bytes reales
            var cerBytes = Convert.FromBase64String(cerBase64);
            var keyBytes = Convert.FromBase64String(keyBase64);

            // 📦 Crear ZIP en memoria
            using var ms = new MemoryStream();
            using (var zip = new System.IO.Compression.ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                var cerEntry = zip.CreateEntry("certificado.cer");
                await using (var cerStream = cerEntry.Open())
                    await cerStream.WriteAsync(cerBytes);

                var keyEntry = zip.CreateEntry("llave.key");
                await using (var keyStream = keyEntry.Open())
                    await keyStream.WriteAsync(keyBytes);
            }

            ms.Position = 0;

            return File(
                ms.ToArray(),
                "application/zip",
                "sellos_digitales.zip"
            );
        }
    }
}

