using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Contracts.Stock
{
    public class PurchaseEntryRequest
    {
        public long SupplierId { get; set; }

        public DateOnly EntryDate { get; set; }

        [StringLength(40)]
        public string? InvoiceNumber { get; set; }

        [StringLength(300)]
        public string? Notes { get; set; }

        public required List<PurchaseEntryItemRequest> Items { get; set; }
    }

    public class PurchaseEntryItemRequest
    {
        public long ProductId { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }
    }
}