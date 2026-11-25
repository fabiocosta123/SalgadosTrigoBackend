namespace SalgadosTrigo.Api.Entities.Stock
{
    public class PurchaseEntryItem
    {
        public long Id { get; set; }

        public long PurchaseEntryId { get; set; }

        public long ProductId { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public decimal LineTotal { get; set; }
    }
}