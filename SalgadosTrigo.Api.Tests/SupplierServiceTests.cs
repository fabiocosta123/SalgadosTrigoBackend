using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Authorization;
using SalgadosTrigo.Api.Contracts.Catalog;
using SalgadosTrigo.Api.Controllers.Catalog;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Identity;
using SalgadosTrigo.Api.Services.Catalog;

namespace SalgadosTrigo.Api.Tests;

public class SupplierServiceTests
{
    [Fact]
    public async Task New_supplier_is_saved_with_document_city_and_creator()
    {
        var (service, db) = await CreateService();

        var result = await service.Register(new SupplierRequest
        {
            Name = "Moinho Sul",
            Document = "12345678000199",
            City = "Curitiba",
            State = "PR"
        }, createdById: 2);

        Assert.NotNull(result.Supplier);
        Assert.Equal("Moinho Sul", result.Supplier.Name);
        Assert.Equal("12345678000199", result.Supplier.Document);
        Assert.Equal("Curitiba", result.Supplier.City);
        Assert.Equal(2, result.Supplier.CreatedById);
        Assert.NotEqual(default, result.Supplier.CreatedAt);

        var stored = await db.Suppliers.SingleAsync();
        Assert.Equal(2, stored.CreatedById);
        Assert.Equal("Curitiba", stored.City);
    }

    [Fact]
    public async Task Query_returns_the_supplier()
    {
        var (service, _) = await CreateService();
        var saved = await service.Register(new SupplierRequest
        {
            Name = "Laticínio Serra",
            Document = "99887766000155",
            City = "Londrina"
        }, createdById: 2);

        var found = await service.Find(saved.Supplier!.Id);

        Assert.NotNull(found);
        Assert.Equal("Laticínio Serra", found.Name);
        Assert.Equal("Londrina", found.City);
        Assert.Equal(2, found.CreatedById);
    }

    [Fact]
    public async Task Repeated_document_is_refused()
    {
        var (service, db) = await CreateService();
        await service.Register(new SupplierRequest
        {
            Name = "Moinho Sul",
            Document = "12345678000199",
            City = "Curitiba"
        }, createdById: 2);

        var repeated = await service.Register(new SupplierRequest
        {
            Name = "Moinho Sul Filial",
            Document = "12345678000199",
            City = "Curitiba"
        }, createdById: 2);

        Assert.Null(repeated.Supplier);
        Assert.Equal(SupplierService.DuplicateDocumentMessage, repeated.Error);
        Assert.Equal(1, await db.Suppliers.CountAsync());
    }

    [Fact]
    public void Supplier_endpoints_use_the_policy_that_refuses_the_seller()
    {
        var authorize = typeof(SuppliersController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal(RolePolicies.Suppliers, authorize.Policy);
    }

    private static async Task<(SupplierService Service, AppDbContext Db)> CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Roles.Add(new Role { Id = 2, Code = "gerente", Name = "Gerente" });
        db.Users.Add(new User
        {
            Id = 2,
            Name = "Bruno",
            Email = "bruno@salgados.com",
            PasswordHash = "hash",
            RoleId = 2,
            IsActive = true
        });
        await db.SaveChangesAsync();
        return (new SupplierService(db), db);
    }
}
