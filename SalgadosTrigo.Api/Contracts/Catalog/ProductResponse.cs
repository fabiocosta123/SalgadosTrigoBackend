namespace SalgadosTrigo.Api.Contracts.Catalog
{
    public class ProductResponse
    {
        public long Id { get; set; }

        public required string Name { get; set; }

        public required string Kind { get; set; }

        public required string Unit { get; set; }

        public string? Code { get; set; }

        public string? Category { get; set; }

        public decimal? CurrentSalePrice { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
