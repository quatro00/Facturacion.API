using Facturacion.API.Data;
using Facturacion.API.Models.Domain;
using Facturacion.API.Models.Dto.Auth;
using Facturacion.API.Models.Dto.Cliente;
using Facturacion.API.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.API.Controllers.Cliente
{
    [Route("api/cliente/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ITokenRepository tokenRepository;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly FacturacionContext _context;

        public AuthController(
            UserManager<IdentityUser> userManager,
            ITokenRepository tokenRepository,
            RoleManager<ApplicationRole> roleManager,
            FacturacionContext context)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.roleManager = roleManager;
            this._context = context;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            // ⚠️ Si usas email como username, esto está bien
            var user = await userManager.FindByNameAsync(model.email);

            if (user == null)
                return BadRequest("Usuario o contraseña incorrectos.");

            var validPassword = await userManager.CheckPasswordAsync(user, model.password);

            if (!validPassword)
                return BadRequest("Usuario o contraseña incorrectos.");

            // ✅ Validar rol Cliente
            var isCliente = await userManager.IsInRoleAsync(user, "Cliente");

            if (!isCliente)
                return BadRequest("Usuario o contraseña incorrectos.");

            // Roles (para el token)
            var roles = await userManager.GetRolesAsync(user);

            var jwtToken = tokenRepository.CreateJwtToken(user, roles.ToList());

            var cuenta = await this._context.Cuenta.Where(x=>x.UserId == user.Id).FirstOrDefaultAsync();
            var response = new LoginResponseDto
            {
                AccessToken = jwtToken,
                TokenType = "bearer",
                User = new UserDto
                {
                    Id = user.Id,
                    Name = cuenta.Nombre,   // aquí luego puedes mapear desde Cuenta
                    Avatar = "",
                    Roles = roles.ToList(),
                    Status = "online",
                    Email = user.Email
                }
            };

            return Ok(response);
        }

        [HttpPost("registro")]
        public async Task<IActionResult> registro([FromBody] RegistroClienteRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (request.email != request.confirmEmail)
                    return BadRequest("El correo y la confirmación no coinciden.");

                if (!request.terminosCondiciones)
                    return BadRequest("Debes aceptar los términos y condiciones.");

                if (await this.userManager.FindByNameAsync(request.username) != null)
                    return BadRequest("El nombre de usuario ya está en uso.");

                if (await this.userManager.FindByEmailAsync(request.email) != null)
                    return BadRequest("El correo electrónico ya está registrado.");

                var user = new IdentityUser
                {
                    UserName = request.username,

                    Email = request.email,
                    EmailConfirmed = false
                };

                var result = await this.userManager.CreateAsync(user, request.password);

                if (!result.Succeeded)
                    return BadRequest(result.Errors.Select(e => e.Description));

                // 🔑 Asignar rol Cliente
                var roleResult = await this.userManager.AddToRoleAsync(user, "Cliente");

                if (!roleResult.Succeeded)
                    return BadRequest(roleResult.Errors.Select(e => e.Description));

                // 3️⃣ Crear Cuenta
                var cuenta = new Cuentum
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Nombre = request.name,
                    FechaRegistro = DateTime.Now,
                };

                _context.Cuenta.Add(cuenta);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            
        }
    }
}
