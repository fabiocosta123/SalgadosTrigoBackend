using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Contracts.Catalog;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Identity;
using SalgadosTrigo.Api.Services.Catalog;

namespace SalgadosTrigo.Api.Tests;

public class CustomerServiceTests
{
    [Fact]
    public async Task New_customer_is_saved_with_segment_prospect_and_creator()
    {
        var (service, db) = await CreateService();

        var result = await service.Register(new CustomerRequest
        {
            Name = "Padaria Sol",
            Segment = "varejo",
            IsProspect = true,
            Document = "12345678901"
        }, createdById: 4);

        Assert.NotNull(result.Customer);
        Assert.Equal("Padaria Sol", result.Customer.Name);
        Assert.Equal("varejo", result.Customer.Segment);
        Assert.True(result.Customer.IsProspect);
        Assert.Equal(4, result.Customer.CreatedById);
        Assert.NotEqual(default, result.Customer.CreatedAt);

        var stored = await db.Customers.SingleAsync();
        Assert.Equal(4, stored.CreatedById);
        Assert.Equal("varejo", stored.Segment);
    }

    [Fact]
    public async Task Query_returns_the_customer_by_id()
    {
        var (service, _) = await CreateService();
        var saved = await service.Register(new CustomerRequest
        {
            Name = "Mercado Norte",
            Segment = "atacado",
            IsProspect = false
        }, createdById: 4);

        var found = await service.Find(saved.Customer!.Id);

        Assert.NotNull(found);
        Assert.Equal("Mercado Norte", found.Name);
        Assert.Equal("atacado", found.Segment);
        Assert.Equal(4, found.CreatedById);
    }

    [Fact]
    public async Task Repeated_document_is_refused()
    {
        var (service, db) = await CreateService();
        await service.Register(new CustomerRequest
        {
            Name = "Padaria Sol",
            Segment = "varejo",
            Document = "12345678901"
        }, createdById: 4);

        var repeated = await service.Register(new CustomerRequest
        {
            Name = "Padaria Sol Filial",
            Segment = "varejo",
            Document = "12345678901"
        }, createdById: 4);

        Assert.Null(repeated.Customer);
        Assert.Equal(CustomerService.DuplicateDocumentMessage, repeated.Error);
        Assert.Equal(1, await db.Customers.CountAsync());
    }

    [Fact]
    public async Task Seller_query_does_not_include_cost_fields()
    {
        var names = typeof(CustomerResponse).GetProperties().Select(property => property.Name).ToArray();

        Assert.DoesNotContain(names, name => name.Contains("Cost", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("CurrentSalePrice", names);
        Assert.Contains("Name", names);
        Assert.Contains("Segment", names);
    }

    private static async Task<(CustomerService Service, AppDbContext Db)> CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Roles.Add(new Role { Id = 4, Code = "vendedor", Name = "Vendedor" });
        db.Users.Add(new User
        {
            Id = 4,
            Name = "Ana",
            Email = "ana@salgados.com",
            PasswordHash = "hash",
            RoleId = 4,
            IsActive = true
        });
        await db.SaveChangesAsync();
        return (new CustomerService(db), db);
    }
}
