using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SalgadosTrigo.Api.Authorization;

namespace SalgadosTrigo.Api.Tests;

public class RolePolicyTests
{
    private readonly IAuthorizationService _authorization = BuildAuthorization();

    [Fact]
    public async Task Seller_is_refused_for_suppliers_costs_and_finance()
    {
        var seller = UserWithRole("vendedor");

        Assert.False((await _authorization.AuthorizeAsync(seller, RolePolicies.Suppliers)).Succeeded);
        Assert.False((await _authorization.AuthorizeAsync(seller, RolePolicies.Costs)).Succeeded);
        Assert.False((await _authorization.AuthorizeAsync(seller, RolePolicies.Finance)).Succeeded);
    }

    [Fact]
    public async Task Manager_can_read_suppliers()
    {
        var manager = UserWithRole("gerente");

        Assert.True((await _authorization.AuthorizeAsync(manager, RolePolicies.Suppliers)).Succeeded);
    }

    [Fact]
    public async Task Administrator_can_manage_payment_terms()
    {
        var administrator = UserWithRole("administrador");

        Assert.True((await _authorization.AuthorizeAsync(administrator, RolePolicies.PaymentTerms)).Succeeded);
    }

    [Fact]
    public async Task Seller_cannot_manage_payment_terms()
    {
        var seller = UserWithRole("vendedor");

        Assert.False((await _authorization.AuthorizeAsync(seller, RolePolicies.PaymentTerms)).Succeeded);
    }

    private static ClaimsPrincipal UserWithRole(string roleCode)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Role, roleCode) },
            authenticationType: "test");

        return new ClaimsPrincipal(identity);
    }

    private static IAuthorizationService BuildAuthorization()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options => options.AddRolePolicies());

        return services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
    }
}