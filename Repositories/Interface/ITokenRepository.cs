using Microsoft.AspNetCore.Identity;

namespace Facturacion.API.Repositories.Interface
{
    public interface ITokenRepository
    {
        string CreateJwtToken(IdentityUser user, List<string> roles, string cuentaId);
    }
}
