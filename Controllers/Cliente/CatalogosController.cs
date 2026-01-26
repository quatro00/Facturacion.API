using AutoMapper;
using Facturacion.API.Helpers;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto.Cliente.Catalogos;
using Facturacion.API.Models.Specifications;
using Facturacion.API.Repositories.Interface;
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
        private readonly IMapper _mapper;

        public CatalogosController(
            IMapper mapper,
            IGenericRepository<CRegimenFiscal> _repoRegimenFiscal,
            IGenericRepository<CCodigoPostal> _repoCodigoPostal
            )
        {
            this._repoRegimenFiscal = _repoRegimenFiscal;
            this._repoCodigoPostal = _repoCodigoPostal;
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
    }
}
