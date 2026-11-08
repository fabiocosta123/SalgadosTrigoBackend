using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Contracts.Catalog;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Catalog;

namespace SalgadosTrigo.Api.Services.Catalog
{
    public class CustomerService(AppDbContext db)
    {
        public const string DuplicateDocumentMessage = "Documento já cadastrado.";

        private static readonly string[] Segments = ["atacado", "varejo"];

        public async Task<IReadOnlyList<CustomerResponse>> List()
        {
            var customers = await db.Customers
                .OrderBy(customer => customer.Name)
                .ThenBy(customer => customer.Id)
                .ToListAsync();

            return customers.Select(ToResponse).ToList();
        }

        public async Task<CustomerResponse?> Find(long id)
        {
            var customer = await db.Customers.SingleOrDefaultAsync(item => item.Id == id);
            return customer is null ? null : ToResponse(customer);
        }

        public async Task<CustomerRegistration> Register(CustomerRequest request, long createdById)
        {
            var name = request.Name.Trim();
            var segment = request.Segment.Trim();
            var document = BlankToNull(request.Document);
            var state = BlankToNull(request.State);

            if (name.Length == 0)
            {
                return CustomerRegistration.Rejected("Informe o nome do cliente.");
            }

            if (!Segments.Contains(segment))
            {
                return CustomerRegistration.Rejected("Informe o segmento: atacado ou varejo.");
            }

            if (state is not null && state.Length != 2)
            {
                return CustomerRegistration.Rejected("Informe a UF com 2 letras.");
            }

            var creatorExists = await db.Users.AnyAsync(user => user.Id == createdById && user.IsActive);
            if (!creatorExists)
            {
                return CustomerRegistration.Rejected("Usuário não encontrado.");
            }

            if (document is not null && await db.Customers.AnyAsync(customer => customer.Document == document))
            {
                return CustomerRegistration.Rejected(DuplicateDocumentMessage);
            }

            var customer = new Customer
            {
                Name = name,
                TradeName = BlankToNull(request.TradeName),
                Document = document,
                Phone = BlankToNull(request.Phone),
                Email = BlankToNull(request.Email),
                Segment = segment,
                Street = BlankToNull(request.Street),
                Number = BlankToNull(request.Number),
                District = BlankToNull(request.District),
                City = BlankToNull(request.City),
                State = state,
                ZipCode = BlankToNull(request.ZipCode),
                IsProspect = request.IsProspect,
                IsActive = request.IsActive,
                Notes = BlankToNull(request.Notes),
                CreatedById = createdById,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Customers.Add(customer);
            await db.SaveChangesAsync();

            return CustomerRegistration.Saved(ToResponse(customer));
        }

        private static string? BlankToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static CustomerResponse ToResponse(Customer customer)
        {
            return new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                TradeName = customer.TradeName,
                Document = customer.Document,
                Phone = customer.Phone,
                Email = customer.Email,
                Segment = customer.Segment,
                Street = customer.Street,
                Number = customer.Number,
                District = customer.District,
                City = customer.City,
                State = customer.State,
                ZipCode = customer.ZipCode,
                IsProspect = customer.IsProspect,
                IsActive = customer.IsActive,
                Notes = customer.Notes,
                CreatedById = customer.CreatedById,
                CreatedAt = customer.CreatedAt
            };
        }
    }

    public class CustomerRegistration
    {
        public CustomerResponse? Customer { get; init; }
        public string? Error { get; init; }

        public static CustomerRegistration Saved(CustomerResponse customer)
        {
            return new CustomerRegistration { Customer = customer };
        }

        public static CustomerRegistration Rejected(string error)
        {
            return new CustomerRegistration { Error = error };
        }
    }
}
