namespace SalgadosTrigo.Api.Contracts.Stock
{
    public class PurchaseEntryResponse
    {
        public long Id { get; set; }

        public long SupplierId { get; set; }

        public DateOnly EntryDate { get; set; }

        public string? InvoiceNumber { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }

        public long CreatedById { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public required List<PurchaseEntryItemResponse> Items { get; set; }
    }

    public class PurchaseEntryItemResponse
    {
        public long Id { get; set; }

        public long ProductId { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public decimal LineTotal { get; set; }
    }
}