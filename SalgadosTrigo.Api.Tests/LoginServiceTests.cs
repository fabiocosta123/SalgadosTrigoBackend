using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Contracts.Identity;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Identity;
using SalgadosTrigo.Api.Services.Identity;

namespace SalgadosTrigo.Api.Tests;

public class LoginServiceTests
{
    [Fact]
    public async Task Active_user_with_correct_password_opens_session()
    {
        var (service, _) = await CreateService("senha-correta", isActive: true);

        var session = await service.Authenticate(new LoginRequest
        {
            Email = "ana@salgados.com",
            Password = "senha-correta"
        });

        Assert.NotNull(session);
        Assert.Equal("Ana", session.Name);
        Assert.Equal(4, session.RoleId);
    }

    [Fact]
    public async Task Wrong_password_does_not_open_session()
    {
        var (service, _) = await CreateService("senha-correta", isActive: true);

        var session = await service.Authenticate(new LoginRequest
        {
            Email = "ana@salgados.com",
            Password = "senha-errada"
        });

        Assert.Null(session);
    }

    [Fact]
    public async Task Inactive_user_does_not_enter()
    {
        var (service, _) = await CreateService("senha-correta", isActive: false);

        var session = await service.Authenticate(new LoginRequest
        {
            Email = "ana@salgados.com",
            Password = "senha-correta"
        });

        Assert.Null(session);
    }

    [Fact]
    public async Task Database_stores_hash_instead_of_typed_password()
    {
        var (_, db) = await CreateService("senha-correta", isActive: true);
        var stored = await db.Users.SingleAsync();

        Assert.NotEqual("senha-correta", stored.PasswordHash);
    }

    private static async Task<(LoginService Service, AppDbContext Db)> CreateService(string password, bool isActive)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        var hasher = new PasswordHasher<User>();

        db.Roles.Add(new Role { Id = 4, Code = "vendedor", Name = "Vendedor" });
        var user = new User
        {
            Name = "Ana",
            Email = "ana@salgados.com",
            PasswordHash = "temporario",
            RoleId = 4,
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow
        };
        user.PasswordHash = hasher.HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (new LoginService(db, hasher), db);
    }
}