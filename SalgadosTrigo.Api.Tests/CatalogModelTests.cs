using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Catalog;

namespace SalgadosTrigo.Api.Tests;

public class CatalogModelTests
{
    private readonly IModel _model;
    private readonly IEntityType _customer;
    private readonly IEntityType _supplier;
    private readonly IEntityType _product;
    private readonly IEntityType _recipe;
    private readonly IEntityType _recipeItem;

    public CatalogModelTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=salgadostrigo_model;Username=model;Password=model")
            .Options;

        using var context = new AppDbContext(options);
        _model = context.GetService<IDesignTimeModel>().Model;
        _customer = _model.FindEntityType(typeof(Customer))!;
        _supplier = _model.FindEntityType(typeof(Supplier))!;
        _product = _model.FindEntityType(typeof(Product))!;
        _recipe = _model.FindEntityType(typeof(Recipe))!;
        _recipeItem = _model.FindEntityType(typeof(RecipeItem))!;
    }

    [Fact]
    public void Model_contains_identity_and_catalog_tables()
    {
        var tables = _model.GetEntityTypes()
            .Select(entity => entity.GetTableName())
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(
            new[] { "customers", "products", "recipe_items", "recipes", "roles", "suppliers", "users" },
            tables);
    }

    [Fact]
    public void Customers_and_suppliers_keep_optional_unique_documents()
    {
        Assert.Equal(160, _customer.FindProperty(nameof(Customer.Name))!.GetMaxLength());
        Assert.True(_customer.FindProperty(nameof(Customer.Document))!.IsNullable);
        Assert.True(_customer.FindProperty(nameof(Customer.State))!.IsFixedLength());
        Assert.Equal(false, _customer.FindProperty(nameof(Customer.IsProspect))!.GetDefaultValue());
        Assert.Equal(true, _customer.FindProperty(nameof(Customer.IsActive))!.GetDefaultValue());
        Assert.Equal("now()", _customer.FindProperty(nameof(Customer.CreatedAt))!.GetDefaultValueSql());
        Assert.True(_customer.GetIndexes().Single(index => index.Properties.Single().Name == nameof(Customer.Document)).IsUnique);

        var customerUser = Assert.Single(_customer.GetForeignKeys());
        Assert.Equal(nameof(Customer.CreatedById), customerUser.Properties.Single().Name);
        Assert.Equal(DeleteBehavior.Restrict, customerUser.DeleteBehavior);

        Assert.True(_supplier.GetIndexes().Single(index => index.Properties.Single().Name == nameof(Supplier.Document)).IsUnique);
        Assert.Equal(DeleteBehavior.Restrict, Assert.Single(_supplier.GetForeignKeys()).DeleteBehavior);
    }

    [Fact]
    public void Products_keep_optional_unique_code_and_sale_price_scale()
    {
        Assert.True(_product.FindProperty(nameof(Product.Code))!.IsNullable);
        Assert.True(_product.GetIndexes().Single(index => index.Properties.Single().Name == nameof(Product.Code)).IsUnique);

        var price = _product.FindProperty(nameof(Product.CurrentSalePrice))!;
        Assert.Equal(14, price.GetPrecision());
        Assert.Equal(2, price.GetScale());
    }

    [Fact]
    public void Recipes_allow_one_sheet_per_finished_product()
    {
        var yield = _recipe.FindProperty(nameof(Recipe.YieldQuantity))!;
        Assert.Equal(14, yield.GetPrecision());
        Assert.Equal(3, yield.GetScale());
        Assert.Equal(1m, yield.GetDefaultValue());
        Assert.True(_recipe.GetIndexes().Single(index => index.Properties.Single().Name == nameof(Recipe.FinishedProductId)).IsUnique);
        Assert.Equal(DeleteBehavior.Restrict, Assert.Single(_recipe.GetForeignKeys()).DeleteBehavior);
    }

    [Fact]
    public void Recipe_items_keep_one_raw_material_per_recipe()
    {
        var quantity = _recipeItem.FindProperty(nameof(RecipeItem.Quantity))!;
        Assert.Equal(14, quantity.GetPrecision());
        Assert.Equal(3, quantity.GetScale());

        var unique = Assert.Single(_recipeItem.GetIndexes(), index => index.IsUnique);
        Assert.Equal(
            new[] { nameof(RecipeItem.RecipeId), nameof(RecipeItem.RawMaterialId) },
            unique.Properties.Select(property => property.Name).ToArray());

        Assert.Equal(
            DeleteBehavior.Cascade,
            _recipeItem.GetForeignKeys().Single(key => key.Properties.Single().Name == nameof(RecipeItem.RecipeId)).DeleteBehavior);
        Assert.Equal(
            DeleteBehavior.Restrict,
            _recipeItem.GetForeignKeys().Single(key => key.Properties.Single().Name == nameof(RecipeItem.RawMaterialId)).DeleteBehavior);
    }
}