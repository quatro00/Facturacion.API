using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Interfaces;
using System.Linq.Expressions;

namespace Facturacion.API.Models.Specifications
{
    public class CCodigoPostalSpecification : ISpecification<CCodigoPostal>
    {
        public Expression<Func<CCodigoPostal, bool>> Criteria { get; }
        public List<string> IncludeStrings { get; set; }
        public CCodigoPostalSpecification(string? codigoPostal)
        {
            Criteria = p =>
            (p.DCodigo == codigoPostal || codigoPostal == null);
        }
    }
}
