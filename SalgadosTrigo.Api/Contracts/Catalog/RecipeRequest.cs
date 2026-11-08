using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Contracts.Catalog
{
    public class RecipeRequest
    {
        public long FinishedProductId { get; set; }

        public decimal YieldQuantity { get; set; } = 1;

        [StringLength(500)]
        public string? Notes { get; set; }

        public required List<RecipeItemRequest> Items { get; set; }
    }

    public class RecipeItemRequest
    {
        public long RawMaterialId { get; set; }

        public decimal Quantity { get; set; }
    }
}
