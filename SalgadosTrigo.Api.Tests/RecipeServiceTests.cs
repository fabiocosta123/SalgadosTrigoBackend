using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Contracts.Catalog;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Catalog;
using SalgadosTrigo.Api.Services.Catalog;

namespace SalgadosTrigo.Api.Tests;

public class RecipeServiceTests
{
    [Fact]
    public async Task Recipe_stores_raw_material_quantity_for_the_yield()
    {
        var (service, db) = await CreateService();

        var result = await service.Register(Sheet(finishedProductId: 3, flour: 0.080m, cheese: 0.020m, yieldQuantity: 1));

        Assert.NotNull(result.Recipe);
        Assert.Equal(3, result.Recipe.FinishedProductId);
        Assert.Equal(1, result.Recipe.YieldQuantity);
        Assert.Equal(2, result.Recipe.Items.Count);
        Assert.Contains(result.Recipe.Items, item => item.RawMaterialId == 1 && item.Quantity == 0.080m);
        Assert.Equal(2, await db.RecipeItems.CountAsync());
    }

    [Fact]
    public async Task Recipe_only_points_to_a_finished_product()
    {
        var (service, db) = await CreateService();

        var result = await service.Register(Sheet(finishedProductId: 1, flour: 0.080m, cheese: 0.020m, yieldQuantity: 1));

        Assert.Null(result.Recipe);
        Assert.Equal(RecipeService.FinishedProductMessage, result.Error);
        Assert.Equal(0, await db.Recipes.CountAsync());
    }

    [Fact]
    public async Task Recipe_item_only_accepts_raw_material()
    {
        var (service, db) = await CreateService();
        var request = Sheet(finishedProductId: 3, flour: 0.080m, cheese: 0.020m, yieldQuantity: 1);
        request.Items[1].RawMaterialId = 3;

        var result = await service.Register(request);

        Assert.Null(result.Recipe);
        Assert.Equal(RecipeService.RawMaterialMessage, result.Error);
        Assert.Equal(0, await db.RecipeItems.CountAsync());
    }

    [Fact]
    public async Task Same_raw_material_does_not_repeat_in_the_recipe()
    {
        var (service, db) = await CreateService();
        var request = Sheet(finishedProductId: 3, flour: 0.080m, cheese: 0.020m, yieldQuantity: 1);
        request.Items[1].RawMaterialId = 1;

        var result = await service.Register(request);

        Assert.Null(result.Recipe);
        Assert.Equal(RecipeService.RepeatedMaterialMessage, result.Error);
        Assert.Equal(0, await db.Recipes.CountAsync());
    }

    private static RecipeRequest Sheet(long finishedProductId, decimal flour, decimal cheese, decimal yieldQuantity)
    {
        return new RecipeRequest
        {
            FinishedProductId = finishedProductId,
            YieldQuantity = yieldQuantity,
            Items =
            [
                new RecipeItemRequest { RawMaterialId = 1, Quantity = flour },
                new RecipeItemRequest { RawMaterialId = 2, Quantity = cheese }
            ]
        };
    }

    private static async Task<(RecipeService Service, AppDbContext Db)> CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Products.AddRange(
            new Product { Id = 1, Name = "Farinha", Kind = "materia_prima", Unit = "kg", IsActive = true },
            new Product { Id = 2, Name = "Queijo", Kind = "materia_prima", Unit = "kg", IsActive = true },
            new Product { Id = 3, Name = "Coxinha", Kind = "produto_acabado", Unit = "un", IsActive = true });
        await db.SaveChangesAsync();
        return (new RecipeService(db), db);
    }
}
