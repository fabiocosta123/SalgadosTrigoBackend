using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Entities.Stock
{
    public class PurchaseEntry
    {
        public long Id { get; set; }

        public long SupplierId { get; set; }

        public DateOnly EntryDate { get; set; }

        [StringLength(40)]
        public string? InvoiceNumber { get; set; }

        public decimal TotalAmount { get; set; }

        [StringLength(300)]
        public string? Notes { get; set; }

        public long CreatedById { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}