using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Contracts.Catalog;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Catalog;

namespace SalgadosTrigo.Api.Services.Catalog
{
    public class RecipeService(AppDbContext db)
    {
        public const string FinishedProductMessage = "A ficha só aponta para produto acabado.";
        public const string RawMaterialMessage = "O item da ficha só aceita matéria-prima.";
        public const string RepeatedMaterialMessage = "A mesma matéria-prima não repete na ficha.";
        public const string AlreadyExistsMessage = "Este produto acabado já tem ficha.";
        public const string NotFoundMessage = "Ficha não encontrada.";

        public async Task<IReadOnlyList<RecipeResponse>> List()
        {
            var recipes = await db.Recipes
                .OrderBy(recipe => recipe.Id)
                .ToListAsync();

            var responses = new List<RecipeResponse>();
            foreach (var recipe in recipes)
            {
                responses.Add(await ToResponse(recipe));
            }

            return responses;
        }

        public async Task<RecipeResponse?> Find(long id)
        {
            var recipe = await db.Recipes.SingleOrDefaultAsync(item => item.Id == id);
            return recipe is null ? null : await ToResponse(recipe);
        }

        public async Task<RecipeRegistration> Register(RecipeRequest request)
        {
            var error = await Validate(request, checkFinishedProduct: true);
            if (error is not null)
            {
                return RecipeRegistration.Rejected(error);
            }

            if (await db.Recipes.AnyAsync(recipe => recipe.FinishedProductId == request.FinishedProductId))
            {
                return RecipeRegistration.Rejected(AlreadyExistsMessage);
            }

            var recipe = new Recipe
            {
                FinishedProductId = request.FinishedProductId,
                YieldQuantity = request.YieldQuantity,
                Notes = BlankToNull(request.Notes),
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();
            AddItems(recipe.Id, request.Items);
            await db.SaveChangesAsync();

            return RecipeRegistration.Saved(await ToResponse(recipe));
        }

        public async Task<RecipeRegistration> Update(long id, RecipeRequest request)
        {
            var recipe = await db.Recipes.SingleOrDefaultAsync(item => item.Id == id);
            if (recipe is null)
            {
                return RecipeRegistration.Rejected(NotFoundMessage);
            }

            if (request.FinishedProductId != recipe.FinishedProductId)
            {
                return RecipeRegistration.Rejected("A ficha não troca de produto acabado.");
            }

            var error = await Validate(request, checkFinishedProduct: false);
            if (error is not null)
            {
                return RecipeRegistration.Rejected(error);
            }

            recipe.YieldQuantity = request.YieldQuantity;
            recipe.Notes = BlankToNull(request.Notes);
            recipe.UpdatedAt = DateTimeOffset.UtcNow;

            var current = await db.RecipeItems.Where(item => item.RecipeId == id).ToListAsync();
            db.RecipeItems.RemoveRange(current);
            AddItems(id, request.Items);
            await db.SaveChangesAsync();

            return RecipeRegistration.Saved(await ToResponse(recipe));
        }

        private void AddItems(long recipeId, IEnumerable<RecipeItemRequest> items)
        {
            foreach (var item in items)
            {
                db.RecipeItems.Add(new RecipeItem
                {
                    RecipeId = recipeId,
                    RawMaterialId = item.RawMaterialId,
                    Quantity = item.Quantity
                });
            }
        }

        private async Task<string?> Validate(RecipeRequest request, bool checkFinishedProduct)
        {
            if (request.YieldQuantity <= 0)
            {
                return "Informe o rendimento da ficha.";
            }

            if (request.Items.Count == 0)
            {
                return "Informe ao menos uma matéria-prima.";
            }

            if (request.Items.Any(item => item.Quantity <= 0))
            {
                return "Informe a quantidade consumida para o rendimento.";
            }

            var rawIds = request.Items.Select(item => item.RawMaterialId).ToList();
            if (rawIds.Distinct().Count() != rawIds.Count)
            {
                return RepeatedMaterialMessage;
            }

            if (checkFinishedProduct)
            {
                var finished = await db.Products.SingleOrDefaultAsync(product => product.Id == request.FinishedProductId);
                if (finished is null || finished.Kind != "produto_acabado")
                {
                    return FinishedProductMessage;
                }
            }

            var materials = await db.Products
                .Where(product => rawIds.Contains(product.Id))
                .ToListAsync();

            if (materials.Count != rawIds.Count)
            {
                return "Matéria-prima não encontrada.";
            }

            if (materials.Any(product => product.Kind != "materia_prima"))
            {
                return RawMaterialMessage;
            }

            return null;
        }

        private async Task<RecipeResponse> ToResponse(Recipe recipe)
        {
            var items = await db.RecipeItems
                .Where(item => item.RecipeId == recipe.Id)
                .OrderBy(item => item.Id)
                .ToListAsync();

            return new RecipeResponse
            {
                Id = recipe.Id,
                FinishedProductId = recipe.FinishedProductId,
                YieldQuantity = recipe.YieldQuantity,
                Notes = recipe.Notes,
                UpdatedAt = recipe.UpdatedAt,
                Items = items.Select(item => new RecipeItemResponse
                {
                    Id = item.Id,
                    RawMaterialId = item.RawMaterialId,
                    Quantity = item.Quantity
                }).ToList()
            };
        }

        private static string? BlankToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }

    public class RecipeRegistration
    {
        public RecipeResponse? Recipe { get; init; }
        public string? Error { get; init; }

        public static RecipeRegistration Saved(RecipeResponse recipe)
        {
            return new RecipeRegistration { Recipe = recipe };
        }

        public static RecipeRegistration Rejected(string error)
        {
            return new RecipeRegistration { Error = error };
        }
    }
}
