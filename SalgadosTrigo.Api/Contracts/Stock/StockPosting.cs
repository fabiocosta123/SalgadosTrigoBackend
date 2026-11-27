using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Contracts.Stock
{
    public class StockPosting
    {
        public long ProductId { get; set; }

        [StringLength(10)]
        public required string Direction { get; set; }
        public decimal Quantity { get; set; }
        [StringLength(30)]
        public required string Reason { get; set; }
        public decimal? UnitCost { get; set; }

        [StringLength(40)]
        public string? ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
        [StringLength(300)]
        public string? Notes { get; set; }
        public long CreatedById { get; set; }
    }
}