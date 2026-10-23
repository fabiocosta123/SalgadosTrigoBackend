using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SalgadosTrigo.Api.Contracts.Identity;
using SalgadosTrigo.Api.Services.Identity;
using System.Security.Claims;

namespace SalgadosTrigo.Api.Controllers.Identity
{
    [ApiController]
    [Route("api/login")]
    public class LoginController(LoginService login) : ControllerBase
    {

        [HttpPost]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var session = await login.Authenticate(request);

            if (session is null)
            {
                return Problem(
                    title: "Não foi possível entrar",
                    detail: "Email ou senha inválidos",
                    statusCode: StatusCodes.Status401Unauthorized
                );
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, session.Id.ToString()),
                new Claim(ClaimTypes.Role, session.RoleCode)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

            return Ok(session);
        }
    }
}
