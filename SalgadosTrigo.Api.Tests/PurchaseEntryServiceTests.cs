using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Authorization;
using SalgadosTrigo.Api.Contracts.Stock;
using SalgadosTrigo.Api.Controllers.Stock;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Catalog;
using SalgadosTrigo.Api.Entities.Identity;
using SalgadosTrigo.Api.Services.Stock;

namespace SalgadosTrigo.Api.Tests;

public class PurchaseEntryServiceTests
{
    [Fact]
    public async Task Purchase_saves_entry_items_balance_and_movement()
    {
        var (service, db) = await CreateService();

        var result = await service.Register(new PurchaseEntryRequest
        {
            SupplierId = 1,
            EntryDate = new DateOnly(2026, 11, 27),
            InvoiceNumber = "NF-100",
            Items =
            [
                new PurchaseEntryItemRequest { ProductId = 1, Quantity = 2m, UnitCost = 4.20m },
                new PurchaseEntryItemRequest { ProductId = 2, Quantity = 0.5m, UnitCost = 30m }
            ]
        }, createdById: 2);

        Assert.NotNull(result.Entry);
        Assert.Equal(1L, result.Entry.SupplierId);
        Assert.Equal(new DateOnly(2026, 11, 27), result.Entry.EntryDate);
        Assert.Equal("NF-100", result.Entry.InvoiceNumber);
        Assert.Equal(23.40m, result.Entry.TotalAmount);
        Assert.Equal(8.40m, result.Entry.Items.Single(item => item.ProductId == 1).LineTotal);
        Assert.Equal(15.00m, result.Entry.Items.Single(item => item.ProductId == 2).LineTotal);

        var flourBalance = await db.StockBalances.SingleAsync(balance => balance.ProductId == 1);
        var cheeseBalance = await db.StockBalances.SingleAsync(balance => balance.ProductId == 2);
        Assert.Equal(2m, flourBalance.Quantity);
        Assert.Equal(0.5m, cheeseBalance.Quantity);

        var movements = await db.StockMovements.OrderBy(movement => movement.ProductId).ToListAsync();
        Assert.Equal(2, movements.Count);
        Assert.All(movements, movement =>
        {
            Assert.Equal("entrada", movement.Direction);
            Assert.Equal("compra", movement.Reason);
            Assert.Equal("purchase_entry", movement.ReferenceType);
            Assert.Equal(result.Entry.Id, movement.ReferenceId);
        });
        Assert.Equal(flourBalance.Quantity, movements[0].BalanceAfter);
        Assert.Equal(cheeseBalance.Quantity, movements[1].BalanceAfter);
    }

    [Fact]
    public async Task Finished_product_item_is_refused()
    {
        var (service, db) = await CreateService();

        var result = await service.Register(new PurchaseEntryRequest
        {
            SupplierId = 1,
            EntryDate = new DateOnly(2026, 11, 27),
            Items =
            [
                new PurchaseEntryItemRequest { ProductId = 3, Quantity = 10m, UnitCost = 1m }
            ]
        }, createdById: 2);

        Assert.Null(result.Entry);
        Assert.Equal("O item da compra só aceita matéria-prima.", result.Error);
        Assert.Equal(0, await db.PurchaseEntries.CountAsync());
        Assert.Equal(0, await db.StockBalances.CountAsync());
        Assert.Equal(0, await db.StockMovements.CountAsync());
    }

    [Fact]
    public void Seller_is_refused_by_the_purchase_policy()
    {
        var authorize = typeof(PurchaseEntriesController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal(RolePolicies.Suppliers, authorize.Policy);
    }

    private static async Task<(PurchaseEntryService Service, AppDbContext Db)> CreateService()
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
        db.Suppliers.Add(new Supplier
        {
            Id = 1,
            Name = "Moinho",
            CreatedById = 2
        });
        db.Products.AddRange(
            new Product { Id = 1, Name = "Farinha", Kind = "materia_prima", Unit = "kg" },
            new Product { Id = 2, Name = "Queijo", Kind = "materia_prima", Unit = "kg" },
            new Product { Id = 3, Name = "Coxinha", Kind = "produto_acabado", Unit = "un" });
        await db.SaveChangesAsync();
        return (new PurchaseEntryService(db, new StockPostingService(db)), db);
    }
}