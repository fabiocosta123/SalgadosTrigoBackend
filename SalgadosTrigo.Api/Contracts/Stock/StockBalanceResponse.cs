namespace SalgadosTrigo.Api.Contracts.Stock
{
    public class StockBalanceResponse
    {
        public long ProductId { get; set; }

        public decimal Quantity { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public required List<StockMovementResponse> Movements { get; set; }
    }

    public class StockMovementResponse
    {
        public decimal Quantity { get; set; }

        public required string Reason { get; set; }

        public decimal? UnitCost { get; set; }

        public decimal BalanceAfter { get; set; }

        public long CreatedById { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}