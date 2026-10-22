using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Identity;

namespace SalgadosTrigo.Api.Tests;

public class IdentityModelTests
{
    private readonly IEntityType _role;
    private readonly IEntityType _user;

    public IdentityModelTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=salgadostrigo_model;Username=model;Password=model")
            .Options;

        using var context = new AppDbContext(options);
        var model = context.GetService<IDesignTimeModel>().Model;
        _role = model.FindEntityType(typeof(Role))!;
        _user = model.FindEntityType(typeof(User))!;
    }

    [Fact]
    public void Model_contains_only_roles_and_users()
    {
        var tables = new[] { _role, _user }
            .Select(entity => entity.GetTableName())
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(new[] { "roles", "users" }, tables);
        Assert.Equal(2, _role.Model.GetEntityTypes().Count());
    }

    [Fact]
    public void Roles_seed_the_five_codes_and_keep_code_unique()
    {
        Assert.Equal(40, _role.FindProperty(nameof(Role.Code))!.GetMaxLength());
        Assert.Equal(80, _role.FindProperty(nameof(Role.Name))!.GetMaxLength());
        Assert.True(_role.GetIndexes().Single(index => index.Properties.Single().Name == nameof(Role.Code)).IsUnique);

        var codes = _role.GetSeedData()
            .Select(row => (string)row[nameof(Role.Code)]!)
            .OrderBy(code => code)
            .ToArray();

        Assert.Equal(
            new[] { "administrador", "financeiro", "funcionario", "gerente", "vendedor" },
            codes);
    }

    [Fact]
    public void Users_start_empty_and_protect_email_and_role()
    {
        Assert.Empty(_user.GetSeedData());
        Assert.Equal(160, _user.FindProperty(nameof(User.Name))!.GetMaxLength());
        Assert.Equal(180, _user.FindProperty(nameof(User.Email))!.GetMaxLength());
        Assert.Equal(500, _user.FindProperty(nameof(User.PasswordHash))!.GetMaxLength());
        Assert.Equal(true, _user.FindProperty(nameof(User.IsActive))!.GetDefaultValue());
        Assert.Equal("now()", _user.FindProperty(nameof(User.CreatedAt))!.GetDefaultValueSql());
        Assert.True(_user.GetIndexes().Single(index => index.Properties.Single().Name == nameof(User.Email)).IsUnique);

        var roleKey = Assert.Single(_user.GetForeignKeys());
        Assert.Equal(nameof(User.RoleId), roleKey.Properties.Single().Name);
        Assert.Equal(DeleteBehavior.Restrict, roleKey.DeleteBehavior);
    }
}
