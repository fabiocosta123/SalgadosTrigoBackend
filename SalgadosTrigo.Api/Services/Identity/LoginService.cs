using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Contracts.Identity;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Identity;

namespace SalgadosTrigo.Api.Services.Identity
{
    public class LoginService(AppDbContext db, IPasswordHasher<User> hasher)
    {
        public async Task<LoginResponse?> Authenticate (LoginRequest request)
        {
            var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !user.IsActive)
            {
                return null;
            }

            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var role = await db.Roles.SingleAsync(r => r.Id == user.RoleId);

            return new LoginResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleCode = role.Code

            };
        }
    }
}
