using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Contracts.Catalog;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Catalog;

namespace SalgadosTrigo.Api.Services.Catalog
{
    public class SupplierService(AppDbContext db)
    {
        public const string DuplicateDocumentMessage = "Documento já cadastrado.";

        public async Task<IReadOnlyList<SupplierResponse>> List()
        {
            var suppliers = await db.Suppliers
                .OrderBy(supplier => supplier.Name)
                .ThenBy(supplier => supplier.Id)
                .ToListAsync();

            return suppliers.Select(ToResponse).ToList();
        }

        public async Task<SupplierResponse?> Find(long id)
        {
            var supplier = await db.Suppliers.SingleOrDefaultAsync(item => item.Id == id);
            return supplier is null ? null : ToResponse(supplier);
        }

        public async Task<SupplierRegistration> Register(SupplierRequest request, long createdById)
        {
            var name = request.Name.Trim();
            var document = BlankToNull(request.Document);
            var state = BlankToNull(request.State);

            if (name.Length == 0)
            {
                return SupplierRegistration.Rejected("Informe o nome do fornecedor.");
            }

            if (state is not null && state.Length != 2)
            {
                return SupplierRegistration.Rejected("Informe a UF com 2 letras.");
            }

            var creatorExists = await db.Users.AnyAsync(user => user.Id == createdById && user.IsActive);
            if (!creatorExists)
            {
                return SupplierRegistration.Rejected("Usuário não encontrado.");
            }

            if (document is not null && await db.Suppliers.AnyAsync(supplier => supplier.Document == document))
            {
                return SupplierRegistration.Rejected(DuplicateDocumentMessage);
            }

            var supplier = new Supplier
            {
                Name = name,
                Document = document,
                Phone = BlankToNull(request.Phone),
                Email = BlankToNull(request.Email),
                City = BlankToNull(request.City),
                State = state,
                IsActive = request.IsActive,
                CreatedById = createdById,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Suppliers.Add(supplier);
            await db.SaveChangesAsync();

            return SupplierRegistration.Saved(ToResponse(supplier));
        }

        private static string? BlankToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static SupplierResponse ToResponse(Supplier supplier)
        {
            return new SupplierResponse
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Document = supplier.Document,
                Phone = supplier.Phone,
                Email = supplier.Email,
                City = supplier.City,
                State = supplier.State,
                IsActive = supplier.IsActive,
                CreatedById = supplier.CreatedById,
                CreatedAt = supplier.CreatedAt
            };
        }
    }

    public class SupplierRegistration
    {
        public SupplierResponse? Supplier { get; init; }
        public string? Error { get; init; }

        public static SupplierRegistration Saved(SupplierResponse supplier)
        {
            return new SupplierRegistration { Supplier = supplier };
        }

        public static SupplierRegistration Rejected(string error)
        {
            return new SupplierRegistration { Error = error };
        }
    }
}
