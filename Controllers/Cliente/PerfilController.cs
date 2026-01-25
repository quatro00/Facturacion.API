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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using System.Text;

namespace Facturacion.API.Controllers.Cliente
{
    [Route("api/cliente/[controller]")]
    [ApiController]
    public class PerfilController : ControllerBase
    {
        private readonly IGenericRepository<RazonSocial> _repoRazonSocial;
        private readonly IMapper _mapper;
        private readonly IConfiguration config;

        public PerfilController(
            IMapper mapper,
            IGenericRepository<RazonSocial> _repoRazonSocial,
            IConfiguration config
            )
        {
            this._repoRazonSocial = _repoRazonSocial;
            this.config = config;
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
                    return BadRequest(new
                    {
                        message = "La cuenta ya tiene una razón social registrada"
                    });
                }
            }
            return Ok();
        }
    }
}
