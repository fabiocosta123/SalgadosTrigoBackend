using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Entities.Catalog
{
    public class Recipe
    {
        public long Id { get; set; }

        public long FinishedProductId { get; set; }

        public decimal YieldQuantity { get; set; } = 1;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}