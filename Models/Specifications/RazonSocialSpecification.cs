using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Interfaces;
using System.Linq.Expressions;

namespace Facturacion.API.Models.Specifications
{
    public class RazonSocialSpecification : ISpecification<RazonSocial>
    {
        public Expression<Func<RazonSocial, bool>> Criteria { get; }
        public List<string> IncludeStrings { get; set; }
        public RazonSocialSpecification(FiltroGlobal filtro, Guid? cuentaId)
        {
            Criteria = p =>
            (p.CuentaId == cuentaId || cuentaId == null) &&
                (filtro.IncluirInactivos || (p.Activo));
        }
    }
}
