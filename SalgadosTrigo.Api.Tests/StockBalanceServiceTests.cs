using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Authorization;
using SalgadosTrigo.Api.Controllers.Stock;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Catalog;
using SalgadosTrigo.Api.Entities.Identity;
using SalgadosTrigo.Api.Entities.Stock;
using SalgadosTrigo.Api.Services.Stock;

namespace SalgadosTrigo.Api.Tests;

public class StockBalanceServiceTests
{
    [Fact]
    public async Task Query_returns_the_stored_quantity_and_the_history()
    {
        var (service, _) = await CreateService();

        var result = await service.Find(1);

        Assert.NotNull(result.Balance);
        Assert.Equal(4m, result.Balance.Quantity);
        Assert.Equal(2, result.Balance.Movements.Count);
        Assert.NotEqual(result.Balance.Movements.Sum(movement => movement.Quantity), result.Balance.Quantity);

        var first = result.Balance.Movements[0];
        Assert.Equal(10m, first.Quantity);
        Assert.Equal("compra", first.Reason);
        Assert.Equal(4.20m, first.UnitCost);
        Assert.Equal(10m, first.BalanceAfter);
        Assert.Equal(2L, first.CreatedById);

        var second = result.Balance.Movements[1];
        Assert.Equal(3m, second.Quantity);
        Assert.Equal("ajuste", second.Reason);
        Assert.Equal(2L, second.CreatedById);
        Assert.True(second.CreatedAt > first.CreatedAt);
    }

    [Fact]
    public async Task Raw_material_without_movement_returns_zero()
    {
        var (service, _) = await CreateService();

        var result = await service.Find(2);

        Assert.NotNull(result.Balance);
        Assert.Null(result.Error);
        Assert.Equal(0m, result.Balance.Quantity);
        Assert.Null(result.Balance.UpdatedAt);
        Assert.Empty(result.Balance.Movements);
    }

    [Fact]
    public async Task Finished_product_is_refused()
    {
        var (service, _) = await CreateService();

        var result = await service.Find(3);

        Assert.Null(result.Balance);
        Assert.Equal(StockBalanceService.FinishedProductMessage, result.Error);
    }

    [Fact]
    public void Seller_is_refused_by_the_cost_policy()
    {
        var authorize = typeof(StockBalancesController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal(RolePolicies.Costs, authorize.Policy);
    }

    private static async Task<(StockBalanceService Service, AppDbContext Db)> CreateService()
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
        db.Products.AddRange(
            new Product { Id = 1, Name = "Farinha", Kind = "materia_prima", Unit = "kg" },
            new Product { Id = 2, Name = "Queijo", Kind = "materia_prima", Unit = "kg" },
            new Product { Id = 3, Name = "Coxinha", Kind = "produto_acabado", Unit = "un" });
        var older = new DateTimeOffset(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);
        var newer = new DateTimeOffset(2026, 11, 27, 12, 0, 0, TimeSpan.Zero);
        db.StockBalances.Add(new StockBalance
        {
            ProductId = 1,
            Quantity = 4m,
            UpdatedAt = newer
        });
        db.StockMovements.AddRange(
            new StockMovement
            {
                ProductId = 1,
                Direction = "entrada",
                Quantity = 10m,
                Reason = "compra",
                UnitCost = 4.20m,
                BalanceAfter = 10m,
                CreatedById = 2,
                CreatedAt = older
            },
            new StockMovement
            {
                ProductId = 1,
                Direction = "saida",
                Quantity = 3m,
                Reason = "ajuste",
                BalanceAfter = 4m,
                CreatedById = 2,
                CreatedAt = newer
            });
        await db.SaveChangesAsync();
        return (new StockBalanceService(db), db);
    }
}