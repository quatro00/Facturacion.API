using Facturacion.API.Data;
using Facturacion.API.Models.Dto.Auth;
using Facturacion.API.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.API.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ITokenRepository tokenRepository;
        private readonly RoleManager<ApplicationRole> roleManager;

        public AuthController(UserManager<IdentityUser> userManager, ITokenRepository tokenRepository, RoleManager<ApplicationRole> roleManager)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.roleManager = roleManager;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            try
            {


                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // O devolver un BadRequest(400) si el error es de entrada
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var user = await userManager.FindByNameAsync(model.email);


            if (user == null)
                return BadRequest("Usuario o contraseña incorrectos.");

            var result = await this.userManager.CheckPasswordAsync(user, model.password);



            if (!result)
                return Unauthorized("Usuario o contraseña incorrectos.");

            var roles = await userManager.GetRolesAsync(user);
            //if (roles.IndexOf("Administrador") == -1)
            //{
            //    ModelState.AddModelError("error", "Email o password incorrecto.");
            //    return ValidationProblem(ModelState);
            //}

            var jwtToken = tokenRepository.CreateJwtToken(user, roles.ToList(), "NA");
            var response = new LoginResponseDto()
            {
                AccessToken = jwtToken,
                TokenType = "bearer",
                User = new UserDto()
                {
                    Id = user.Id,
                    Name = "",
                    Avatar = "",
                    Roles = roles.ToList(),
                    Status = "online",
                    Email = user.Email,
                }
            };


            return Ok(response);
        }
    }
}
