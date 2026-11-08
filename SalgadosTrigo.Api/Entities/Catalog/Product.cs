using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Entities.Catalog
{
    public class Product
    {
        public long Id { get; set; }

        [StringLength(160)]
        public required string Name { get; set; }

        [StringLength(20)]
        public required string Kind { get; set; }

        [StringLength(10)]
        public required string Unit { get; set; }

        [StringLength(40)]
        public string? Code { get; set; }

        [StringLength(80)]
        public string? Category { get; set; }

        public decimal? CurrentSalePrice { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; }
    }
}