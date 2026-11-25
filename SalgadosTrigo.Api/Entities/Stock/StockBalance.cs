namespace SalgadosTrigo.Api.Entities.Stock
{
    public class StockBalance
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}