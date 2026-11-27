using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Contracts.Stock;
using SalgadosTrigo.Api.Data;

namespace SalgadosTrigo.Api.Services.Stock
{
    public class StockBalanceService(AppDbContext db)
    {
        public const string NotFoundMessage = "Matéria-prima não encontrada.";
        public const string FinishedProductMessage = "A consulta deste saldo é só de matéria-prima.";
        public async Task<StockBalanceQuery> Find(long productId)
        {
            var product = await db.Products.SingleOrDefaultAsync(product => product.Id == productId);
            if (product is null)
            {
                return StockBalanceQuery.Rejected(NotFoundMessage);
            }

            if (product.Kind != "materia_prima")
            {
                return StockBalanceQuery.Rejected(FinishedProductMessage);
            }

            var balance = await db.StockBalances.SingleOrDefaultAsync(item => item.ProductId == productId);
            var movements = await db.StockMovements
                .Where(movement => movement.ProductId == productId)
                .OrderBy(movement => movement.CreatedAt)
                .ThenBy(movement => movement.Id)
                .ToListAsync();

            return StockBalanceQuery.Found(new StockBalanceResponse
            {
                ProductId = productId,
                Quantity = balance?.Quantity ?? 0m,
                UpdatedAt = balance?.UpdatedAt,
                Movements = movements.Select(movement => new StockMovementResponse
                {
                    Quantity = movement.Quantity,
                    Reason = movement.Reason,
                    UnitCost = movement.UnitCost,
                    BalanceAfter = movement.BalanceAfter,
                    CreatedById = movement.CreatedById,
                    CreatedAt = movement.CreatedAt
                }).ToList()
            });
        }
    }

    public class StockBalanceQuery
    {
        public StockBalanceResponse? Balance { get; init; }
        public string? Error { get; init; }

        public static StockBalanceQuery Found(StockBalanceResponse balance)
        {
            return new StockBalanceQuery { Balance = balance };
        }

        public static StockBalanceQuery Rejected(string error)
        {
            return new StockBalanceQuery { Error = error };
        }
    }
}