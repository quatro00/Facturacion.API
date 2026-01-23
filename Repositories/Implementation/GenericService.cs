using Facturacion.API.Models.Domain;
using Facturacion.API.Repositories.Interface;

namespace Facturacion.API.Repositories.Implementation
{
    public class GenericService
    {
        private readonly IGenericRepository<Organizacion> _orgRepo;
        private readonly IGenericRepository<Sistema> _sisRepo;

        public GenericService(
        IGenericRepository<Organizacion> orgRepo,
        IGenericRepository<Sistema> sisRepo)
        {
            _orgRepo = orgRepo;
            _sisRepo = sisRepo;
        }
    }
}
