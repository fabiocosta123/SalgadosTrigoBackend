using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Contracts.Catalog;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Services.Catalog;

namespace SalgadosTrigo.Api.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task Raw_material_and_finished_product_are_saved_with_different_kinds()
    {
        var (service, db) = CreateService();

        var flour = await service.Register(new ProductRequest
        {
            Name = "Farinha",
            Kind = "materia_prima",
            Unit = "kg",
            Code = "FAR-1"
        });
        var coxinha = await service.Register(new ProductRequest
        {
            Name = "Coxinha",
            Kind = "produto_acabado",
            Unit = "un",
            Code = "COX-1",
            Category = "coxinha",
            CurrentSalePrice = 8.50m
        });

        Assert.NotNull(flour.Product);
        Assert.NotNull(coxinha.Product);
        Assert.NotEqual(flour.Product.Kind, coxinha.Product.Kind);
        Assert.Equal(2, await db.Products.CountAsync());
        Assert.Equal(8.50m, coxinha.Product.CurrentSalePrice);
        Assert.True(flour.Product.IsActive);
    }

    [Fact]
    public async Task Repeated_code_is_refused()
    {
        var (service, db) = CreateService();
        await service.Register(new ProductRequest
        {
            Name = "Farinha",
            Kind = "materia_prima",
            Unit = "kg",
            Code = "FAR-1"
        });

        var repeated = await service.Register(new ProductRequest
        {
            Name = "Farinha especial",
            Kind = "materia_prima",
            Unit = "kg",
            Code = "FAR-1"
        });

        Assert.Null(repeated.Product);
        Assert.Equal(ProductService.DuplicateCodeMessage, repeated.Error);
        Assert.Equal(1, await db.Products.CountAsync(product => product.Code == "FAR-1"));
    }

    [Fact]
    public async Task Product_without_kind_is_refused()
    {
        var (service, db) = CreateService();

        var result = await service.Register(new ProductRequest
        {
            Name = "Farinha",
            Kind = " ",
            Unit = "kg"
        });

        Assert.Null(result.Product);
        Assert.Equal(0, await db.Products.CountAsync());
    }

    [Fact]
    public async Task Product_without_unit_is_refused()
    {
        var (service, db) = CreateService();

        var result = await service.Register(new ProductRequest
        {
            Name = "Farinha",
            Kind = "materia_prima",
            Unit = ""
        });

        Assert.Null(result.Product);
        Assert.Equal(0, await db.Products.CountAsync());
    }

    private static (ProductService Service, AppDbContext Db) CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        return (new ProductService(db), db);
    }
}
