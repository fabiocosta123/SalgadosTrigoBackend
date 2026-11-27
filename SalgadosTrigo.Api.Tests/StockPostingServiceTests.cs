using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SalgadosTrigo.Api.Contracts.Stock;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Catalog;
using SalgadosTrigo.Api.Entities.Identity;
using SalgadosTrigo.Api.Services.Stock;
using SalgadosTrigo.Api.Entities.Stock;

namespace SalgadosTrigo.Api.Tests;

public class StockPostingServiceTests
{
    [Fact]
    public async Task Entry_saves_balance_and_movement_together()
    {
        var (service, db) = await CreateService();

        var result = await service.Post(Posting(2.5m));

        Assert.Null(result.Error);
        var balance = await db.StockBalances.SingleAsync();
        var movement = await db.StockMovements.SingleAsync();
        Assert.Equal(2.5m, balance.Quantity);
        Assert.Equal("entrada", movement.Direction);
        Assert.Equal(2.5m, movement.Quantity);
        Assert.Equal("compra", movement.Reason);
        Assert.Equal(4.20m, movement.UnitCost);
        Assert.Equal(2L, movement.CreatedById);
        Assert.Equal(balance.Quantity, movement.BalanceAfter);
        Assert.Equal(balance.Quantity, result.BalanceAfter);
    }

    [Fact]
    public async Task Second_entry_keeps_balance_after_equal_to_the_new_quantity()
    {
        var (service, db) = await CreateService();
        await service.Post(Posting(2m));

        var result = await service.Post(Posting(1.5m));

        var balance = await db.StockBalances.SingleAsync();
        var movement = await db.StockMovements.OrderBy(item => item.Id).LastAsync();
        Assert.Equal(3.5m, balance.Quantity);
        Assert.Equal(3.5m, movement.BalanceAfter);
        Assert.Equal(balance.Quantity, result.BalanceAfter);
    }

    [Fact]
    public async Task Movement_failure_keeps_the_previous_balance()
    {
        var databaseName = Guid.NewGuid().ToString();
        var (service, _) = await CreateService(databaseName);
        await service.Post(Posting(4m));

        var (failing, _) = await CreateService(databaseName, new MovementSaveFailure(), seed: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => failing.Post(Posting(1m)));

        await using var check = new AppDbContext(Options(databaseName));
        var balance = await check.StockBalances.SingleAsync();
        Assert.Equal(4m, balance.Quantity);
        Assert.Equal(1, await check.StockMovements.CountAsync());
    }

    private static StockPosting Posting(decimal quantity)
    {
        return new StockPosting
        {
            ProductId = 1,
            Direction = "entrada",
            Quantity = quantity,
            Reason = "compra",
            UnitCost = 4.20m,
            ReferenceType = "purchase_entry",
            ReferenceId = 10,
            CreatedById = 2
        };
    }

    private static async Task<(StockPostingService Service, AppDbContext Db)> CreateService(
        string? databaseName = null,
        SaveChangesInterceptor? interceptor = null,
        bool seed = true)
    {
        databaseName ??= Guid.NewGuid().ToString();
        var db = new AppDbContext(Options(databaseName, interceptor));
        if (seed)
        {
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
            db.Products.Add(new Product
            {
                Id = 1,
                Name = "Farinha",
                Kind = "materia_prima",
                Unit = "kg"
            });
            await db.SaveChangesAsync();
        }

        return (new StockPostingService(db), db);
    }

    private static DbContextOptions<AppDbContext> Options(string databaseName, SaveChangesInterceptor? interceptor = null)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName);
        if (interceptor is not null)
        {
            builder.AddInterceptors(interceptor);
        }

        return builder.Options;
    }

    private sealed class MovementSaveFailure : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var addedMovement = eventData.Context!.ChangeTracker
                .Entries<StockMovement>()
                .Any(entry => entry.State == EntityState.Added);
            if (addedMovement)
            {
                throw new InvalidOperationException("falha no movimento");
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}