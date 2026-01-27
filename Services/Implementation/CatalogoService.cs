using AutoMapper;
using Facturacion.API.Data;
using Facturacion.API.Models.Dto.Cliente.Catalogos;
using Facturacion.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.API.Services.Implementation
{
    public class CatalogoService : ICatalogoService
    {
        private readonly FacturacionContext _context;
        private readonly IMapper _mapper;

        public CatalogoService(
            IMapper mapper,
            FacturacionContext context
            )
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<List<GetClaveUnidadDto>> BuscarClavesUnidadAsync(string search, int take = 20)
        {
            if (string.IsNullOrWhiteSpace(search))
                return new List<GetClaveUnidadDto>();

            search = search.Trim();
            if (search.Length < 2)
                return new List<GetClaveUnidadDto>();

            take = Math.Clamp(take, 1, 50);

            // OJO: asumo que tienes DbSet<CClaveUnidad> en tu contexto
            var query = _context.CClaveUnidads.AsNoTracking();

            // Para clave tipo "H87", "E48", "KGM": mejor prefijo (LIKE 'H8%')
            // Para texto: startswith en nombre
            bool looksLikeKey = search.Length <= 6 && search.All(ch => char.IsLetterOrDigit(ch));

            if (looksLikeKey)
            {
                return await query
                    .Where(x => EF.Functions.Like(x.CClaveUnidad1, search + "%")
                             || x.Nombre.StartsWith(search))
                    .OrderBy(x => x.CClaveUnidad1)
                    .Take(take)
                    .Select(x => new GetClaveUnidadDto
                    {
                        CClaveUnidad = x.CClaveUnidad1,
                        Nombre = x.Nombre
                    })
                    .ToListAsync();
            }

            return await query
                .Where(x => x.Nombre.StartsWith(search))
                .OrderBy(x => x.CClaveUnidad1)
                .Take(take)
                .Select(x => new GetClaveUnidadDto
                {
                    CClaveUnidad = x.CClaveUnidad1,
                    Nombre = x.Nombre
                })
                .ToListAsync();
        }

        public async Task<List<GetConceptosDto>> BuscarConceptosAsync(string search, int take = 20)
        {
            if (string.IsNullOrWhiteSpace(search))
                return new List<GetConceptosDto>();

            search = search.Trim();
            if (search.Length < 3)
                return new List<GetConceptosDto>();

            take = Math.Clamp(take, 1, 50);

            var query = _context.CConceptos.AsNoTracking();

            bool isDigitsOnly = search.All(char.IsDigit);

            if (isDigitsOnly)
            {
                // Prefijo en clave: usa índice en CClaveProdServ
                return await query
                    .Where(x => EF.Functions.Like(x.CClaveProdServ, search + "%"))
                    .OrderBy(x => x.CClaveProdServ)
                    .Take(take)
                    .Select(x => new GetConceptosDto
                    {
                        CClaveProdServ = x.CClaveProdServ,
                        Descripcion = x.Descripcion
                    })
                    .ToListAsync();
            }

            // Texto: StartsWith para evitar %texto% (mejor rendimiento)
            return await query
                .Where(x =>
                    x.Descripcion.StartsWith(search) ||
                    x.CClaveProdServ.StartsWith(search))
                .OrderBy(x => x.CClaveProdServ)
                .Take(take)
                .Select(x => new GetConceptosDto
                {
                    CClaveProdServ = x.CClaveProdServ,
                    Descripcion = x.Descripcion
                })
                .ToListAsync();
        }
    }
}
