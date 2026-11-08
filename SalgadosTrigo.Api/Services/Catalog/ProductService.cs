using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Contracts.Catalog;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Catalog;

namespace SalgadosTrigo.Api.Services.Catalog
{
    public class ProductService(AppDbContext db)
    {
        public const string DuplicateCodeMessage = "Código já cadastrado.";

        private static readonly string[] Kinds = ["materia_prima", "produto_acabado"];
        private static readonly string[] Units = ["un", "kg", "g", "l"];

        public async Task<IReadOnlyList<ProductResponse>> List()
        {
            var products = await db.Products
                .OrderBy(product => product.Name)
                .ThenBy(product => product.Id)
                .ToListAsync();

            return products.Select(ToResponse).ToList();
        }

        public async Task<ProductResponse?> Find(long id)
        {
            var product = await db.Products.SingleOrDefaultAsync(item => item.Id == id);
            return product is null ? null : ToResponse(product);
        }

        public async Task<ProductRegistration> Register(ProductRequest request)
        {
            var name = request.Name.Trim();
            var kind = request.Kind.Trim();
            var unit = request.Unit.Trim();
            var code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();
            var category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();

            if (name.Length == 0)
            {
                return ProductRegistration.Rejected("Informe o nome do produto.");
            }

            if (!Kinds.Contains(kind))
            {
                return ProductRegistration.Rejected("Informe o tipo do produto: materia_prima ou produto_acabado.");
            }

            if (!Units.Contains(unit))
            {
                return ProductRegistration.Rejected("Informe a unidade: un, kg, g ou l.");
            }

            if (code is not null && await db.Products.AnyAsync(product => product.Code == code))
            {
                return ProductRegistration.Rejected(DuplicateCodeMessage);
            }

            var product = new Product
            {
                Name = name,
                Kind = kind,
                Unit = unit,
                Code = code,
                Category = category,
                CurrentSalePrice = request.CurrentSalePrice,
                IsActive = request.IsActive,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            return ProductRegistration.Saved(ToResponse(product));
        }

        private static ProductResponse ToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Kind = product.Kind,
                Unit = product.Unit,
                Code = product.Code,
                Category = product.Category,
                CurrentSalePrice = product.CurrentSalePrice,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt
            };
        }
    }

    public class ProductRegistration
    {
        public ProductResponse? Product { get; init; }
        public string? Error { get; init; }

        public static ProductRegistration Saved(ProductResponse product)
        {
            return new ProductRegistration { Product = product };
        }

        public static ProductRegistration Rejected(string error)
        {
            return new ProductRegistration { Error = error };
        }
    }
}
