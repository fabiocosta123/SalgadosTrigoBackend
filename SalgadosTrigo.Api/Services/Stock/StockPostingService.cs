using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Contracts;
using SalgadosTrigo.Api.Contracts.Stock;
using SalgadosTrigo.Api.Data;
using SalgadosTrigo.Api.Entities.Catalog;
using SalgadosTrigo.Api.Entities.Stock;

namespace SalgadosTrigo.Api.Services.Stock
{
    public class StockPostingService(AppDbContext db)
    {
        private static readonly string[] Directions = ["entrada", "saida"];
        private static readonly string[] Reasons = ["compra", "producao_consumo", "producao_entrada", "venda", "ajuste"];
        private static readonly string[] ReferenceTypes = ["purchase_entry", "production_order", "order"];

        public async Task<StockPostingResult> Post(StockPosting posting)
        {
            var direction = posting.Direction.Trim();
            var reason = posting.Reason.Trim();
            var referenceType = BlankToNull(posting.ReferenceType);
            var notes = BlankToNull(posting.Notes);

            if (!Directions.Contains(direction))
            {
                return StockPostingResult.Rejected("Informe a direção: entrada ou saída");

            }

            if (posting.Quantity <= 0)
            {
                return StockPostingResult.Rejected("Informe a quantidade do movimento.");
            }

            if (!Reasons.Contains(reason))
            {
                return StockPostingResult.Rejected("Informe o motivo do movimento: compra, consumo de produção, entrada de produção, venda ou ajuste.");
            }

            if (referenceType is not null && !ReferenceTypes.Contains(referenceType))
            {
                return StockPostingResult.Rejected("Informe a origem do movimento: compra, produção ou pedido.");
            }

            var productExists = await db.Products.AnyAsync(product => product.Id == posting.ProductId);
            if (!productExists)
            {
                return StockPostingResult.Rejected("Produto não encontrado.");
            }

            var creatorExists = await db.Users.AnyAsync(user => user.Id == posting.CreatedById && user.IsActive);
            if (!creatorExists)
            {
                return StockPostingResult.Rejected("Usuário não encontrado.");
            }

            var balance = await db.StockBalances.SingleOrDefaultAsync(item => item.ProductId == posting.ProductId);
            var current = balance?.Quantity ?? 0m;
            var next = direction == "entrada" ? current + posting.Quantity : current - posting.Quantity;

            if (next < 0)
            {
                return StockPostingResult.Rejected("O saldo não pode ficar negativo.");
            }

            await using var transaction = db.Database.IsRelational()
                ? await db.Database.BeginTransactionAsync()
                : null;

            var now = DateTimeOffset.UtcNow;
            if (balance is null)
            {
                balance = new StockBalance
                {
                    ProductId = posting.ProductId,
                    Quantity = next,
                    UpdatedAt = now
                };
                db.StockBalances.Add(balance);
            }
            else
            {
                balance.Quantity = next;
                balance.UpdatedAt = now;
            }
            var movement = new StockMovement
            {
                ProductId = posting.ProductId,
                Direction = direction,
                Quantity = posting.Quantity,
                Reason = reason,
                UnitCost = posting.UnitCost,
                ReferenceType = referenceType,
                ReferenceId = posting.ReferenceId,
                BalanceAfter = next,
                Notes = notes,
                CreatedById = posting.CreatedById,
                CreatedAt = now
            };
            db.StockMovements.Add(movement);
            await db.SaveChangesAsync();
            if (transaction is not null)
            {
                await transaction.CommitAsync();
            }
            return StockPostingResult.Saved(movement.Id, balance.Quantity);
        }
        private static string? BlankToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
    public class StockPostingResult
    {
        public long? MovementId { get; init; }
        public decimal? BalanceAfter { get; init; }
        public string? Error { get; init; }
        public static StockPostingResult Saved(long movementId, decimal balanceAfter)
        {
            return new StockPostingResult { MovementId = movementId, BalanceAfter = balanceAfter };
        }
        public static StockPostingResult Rejected(string error)
        {
            return new StockPostingResult { Error = error };
        }

    }

}