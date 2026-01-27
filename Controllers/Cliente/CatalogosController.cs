using AutoMapper;
using Facturacion.API.Helpers;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto.Cliente.Catalogos;
using Facturacion.API.Models.Specifications;
using Facturacion.API.Repositories.Interface;
using Facturacion.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace Facturacion.API.Controllers.Cliente
{
    [Route("api/cliente/[controller]")]
    [ApiController]
    public class CatalogosController : ControllerBase
    {
        private readonly IGenericRepository<CRegimenFiscal> _repoRegimenFiscal;
        private readonly IGenericRepository<CCodigoPostal> _repoCodigoPostal;
        private readonly IGenericRepository<CMetodoPago> _repoMetodoPgo;
        private readonly IGenericRepository<CFormaPago> _repoFormaPago;
        private readonly IGenericRepository<CMonedum> _repoMoneda;
        private readonly IGenericRepository<CExportacion> _repoExportacion;
        private readonly IGenericRepository<CUsoCfdi> _repoUsoCfdi;
        private readonly ICatalogoService _catalogoService;
        private readonly IMapper _mapper;

        public CatalogosController(
            IMapper mapper,
            IGenericRepository<CRegimenFiscal> _repoRegimenFiscal,
            IGenericRepository<CCodigoPostal> _repoCodigoPostal,
            IGenericRepository<CMetodoPago> _repoMetodoPgo,
            IGenericRepository<CFormaPago> _repoFormaPago,
            IGenericRepository<CMonedum> _repoMoneda,
            IGenericRepository<CExportacion> _repoExportacion,
            IGenericRepository<CUsoCfdi> _repoUsoCfdi,
            ICatalogoService _catalogoService
            )
        {
            this._repoRegimenFiscal = _repoRegimenFiscal;
            this._repoCodigoPostal = _repoCodigoPostal;
            this._repoMetodoPgo = _repoMetodoPgo;
            this._repoFormaPago = _repoFormaPago;
            this._repoMoneda = _repoMoneda;
            this._repoExportacion = _repoExportacion;
            this._repoUsoCfdi = _repoUsoCfdi;
            this._catalogoService = _catalogoService;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("GetRegimenFiscal")]
        public async Task<IActionResult> GetRegimenFiscal()
        {
            var userid = User.GetId();
            var data = await this._repoRegimenFiscal.ListAsync();
            var dto = _mapper.Map<IEnumerable<ClienteGetRegimenFiscal>>(data);
            return Ok(dto);
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("GetMunicipio")]
        public async Task<IActionResult> GetMunicipio([FromQuery] string codigoPostal)
        {
            var spec = new CCodigoPostalSpecification(codigoPostal);
            var data = await this._repoCodigoPostal.ListAsync(spec);

            GetMunicipioResponse res = new GetMunicipioResponse();
            res.colonia = new List<string>();
            foreach (var item in data)
            {
                res.estado = item.DEstado;
                res.municipio = item.DMnpio;
                res.colonia.Add(item.DAsenta);
            }

            return Ok(res);
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("GetMetodoPago")]
        public async Task<IActionResult> GetMetodoPago()
        {
            var data = await this._repoMetodoPgo.ListAsync();
            var dto = _mapper.Map<IEnumerable<GetMetodoPagoDto>>(data);
            return Ok(dto);
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("GetFormaPago")]
        public async Task<IActionResult> GetFormaPago()
        {
            var data = await this._repoFormaPago.ListAsync();
            var dto = _mapper.Map<IEnumerable<GetFormaPagoDto>>(data);
            return Ok(dto);
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("GetMoneda")]
        public async Task<IActionResult> GetMoneda()
        {
            var data = await this._repoMoneda.ListAsync();
            var dto = _mapper.Map<IEnumerable<GetMonedaDto>>(data);
            return Ok(dto);
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("GetExportacion")]
        public async Task<IActionResult> GetExportacion()
        {
            var data = await this._repoExportacion.ListAsync();
            var dto = _mapper.Map<IEnumerable<GetExportacionDto>>(data);
            return Ok(dto);
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("GetUsoCfdi")]
        public async Task<IActionResult> GetUsoCfdi()
        {
            var data = await this._repoUsoCfdi.ListAsync();
            var dto = _mapper.Map<IEnumerable<GetUsoCfdiDto>>(data);
            return Ok(dto);
        }

        [HttpGet("GetConceptos")]
        public async Task<IActionResult> GetConceptos([FromQuery] string search, [FromQuery] int take = 20)
        {
            var data = await _catalogoService.BuscarConceptosAsync(search, take);
            return Ok(data);
        }

        [HttpGet("GetClaveUnidad")]
        public async Task<IActionResult> GetClaveUnidad([FromQuery] string search, [FromQuery] int take = 20)
        {
            var data = await _catalogoService.BuscarClavesUnidadAsync(search, take);
            return Ok(data);
        }
    }
}
