namespace SalgadosTrigo.Api.Contracts.Catalog
{
    public class RecipeResponse
    {
        public long Id { get; set; }

        public long FinishedProductId { get; set; }

        public decimal YieldQuantity { get; set; }

        public string? Notes { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public required List<RecipeItemResponse> Items { get; set; }
    }

    public class RecipeItemResponse
    {
        public long Id { get; set; }

        public long RawMaterialId { get; set; }

        public decimal Quantity { get; set; }
    }
}
