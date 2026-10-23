using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Contracts.Identity
{
    public class LoginRequest
    {
        [StringLength(180)]
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
