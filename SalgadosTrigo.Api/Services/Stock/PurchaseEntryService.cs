using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Contracts.Stock;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Stock;

namespace SalgadosTrigo.Api.Services.Stock
{
    public class PurchaseEntryService(AppDbContext db, StockPostingService stock)
    {
        public async Task<PurchaseEntryRegistration> Register(PurchaseEntryRequest request, long createdById)
        {
            if (request.EntryDate == default)
            {
                return PurchaseEntryRegistration.Rejected("Informe a data da compra.");
            }

            if (request.Items is null || request.Items.Count == 0)
            {
                return PurchaseEntryRegistration.Rejected("Informe ao menos uma matéria-prima.");
            }

            var supplierExists = await db.Suppliers.AnyAsync(supplier => supplier.Id == request.SupplierId);
            if (!supplierExists)
            {
                return PurchaseEntryRegistration.Rejected("Fornecedor não encontrado.");
            }

            var creatorExists = await db.Users.AnyAsync(user => user.Id == createdById && user.IsActive);
            if (!creatorExists)
            {
                return PurchaseEntryRegistration.Rejected("Usuário não encontrado.");
            }

            var productIds = request.Items.Select(item => item.ProductId).Distinct().ToArray();
            var products = await db.Products
                .Where(product => productIds.Contains(product.Id))
                .ToListAsync();

            var lines = new List<(PurchaseEntryItemRequest Item, decimal LineTotal)>();
            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                {
                    return PurchaseEntryRegistration.Rejected("Informe a quantidade do item.");
                }

                if (item.UnitCost < 0)
                {
                    return PurchaseEntryRegistration.Rejected("Informe o custo do item.");
                }

                var product = products.SingleOrDefault(candidate => candidate.Id == item.ProductId);
                if (product is null)
                {
                    return PurchaseEntryRegistration.Rejected("Matéria-prima não encontrada.");
                }

                if (product.Kind != "materia_prima")
                {
                    return PurchaseEntryRegistration.Rejected("O item da compra só aceita matéria-prima.");
                }

                var lineTotal = decimal.Round(item.Quantity * item.UnitCost, 2, MidpointRounding.AwayFromZero);
                lines.Add((item, lineTotal));
            }

            var ownsTransaction = db.Database.IsRelational();
            await using var transaction = ownsTransaction
                ? await db.Database.BeginTransactionAsync()
                : null;

            var now = DateTimeOffset.UtcNow;
            var entry = new PurchaseEntry
            {
                SupplierId = request.SupplierId,
                EntryDate = request.EntryDate,
                InvoiceNumber = BlankToNull(request.InvoiceNumber),
                TotalAmount = lines.Sum(line => line.LineTotal),
                Notes = BlankToNull(request.Notes),
                CreatedById = createdById,
                CreatedAt = now
            };
            db.PurchaseEntries.Add(entry);
            await db.SaveChangesAsync();

            foreach (var line in lines)
            {
                db.PurchaseEntryItems.Add(new PurchaseEntryItem
                {
                    PurchaseEntryId = entry.Id,
                    ProductId = line.Item.ProductId,
                    Quantity = line.Item.Quantity,
                    UnitCost = line.Item.UnitCost,
                    LineTotal = line.LineTotal
                });
            }
            await db.SaveChangesAsync();

            foreach (var line in lines)
            {
                var posted = await stock.Post(new StockPosting
                {
                    ProductId = line.Item.ProductId,
                    Direction = "entrada",
                    Quantity = line.Item.Quantity,
                    Reason = "compra",
                    UnitCost = line.Item.UnitCost,
                    ReferenceType = "purchase_entry",
                    ReferenceId = entry.Id,
                    CreatedById = createdById
                });

                if (posted.Error is not null)
                {
                    if (transaction is not null)
                    {
                        await transaction.RollbackAsync();
                    }

                    return PurchaseEntryRegistration.Rejected(posted.Error);
                }
            }

            if (transaction is not null)
            {
                await transaction.CommitAsync();
            }

            return PurchaseEntryRegistration.Saved(await ToResponse(entry));
        }

        private async Task<PurchaseEntryResponse> ToResponse(PurchaseEntry entry)
        {
            var items = await db.PurchaseEntryItems
                .Where(item => item.PurchaseEntryId == entry.Id)
                .OrderBy(item => item.Id)
                .ToListAsync();

            return new PurchaseEntryResponse
            {
                Id = entry.Id,
                SupplierId = entry.SupplierId,
                EntryDate = entry.EntryDate,
                InvoiceNumber = entry.InvoiceNumber,
                TotalAmount = entry.TotalAmount,
                Notes = entry.Notes,
                CreatedById = entry.CreatedById,
                CreatedAt = entry.CreatedAt,
                Items = items.Select(item => new PurchaseEntryItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    LineTotal = item.LineTotal
                }).ToList()
            };
        }

        private static string? BlankToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }

    public class PurchaseEntryRegistration
    {
        public PurchaseEntryResponse? Entry { get; init; }
        public string? Error { get; init; }

        public static PurchaseEntryRegistration Saved(PurchaseEntryResponse entry)
        {
            return new PurchaseEntryRegistration { Entry = entry };
        }

        public static PurchaseEntryRegistration Rejected(string error)
        {
            return new PurchaseEntryRegistration { Error = error };
        }
    }
}