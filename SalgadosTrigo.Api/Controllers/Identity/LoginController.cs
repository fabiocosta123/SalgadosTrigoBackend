using Microsoft.AspNetCore.Mvc;
using SalgadosTrigo.Api.Services.Identity;
using SalgadosTrigo.Api.Contracts.Identity;

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

            return Ok(session);
        }
    }
}
