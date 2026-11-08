namespace SalgadosTrigo.Api.Contracts.Catalog
{
    public class CustomerResponse
    {
        public long Id { get; set; }

        public required string Name { get; set; }

        public string? TradeName { get; set; }

        public string? Document { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public required string Segment { get; set; }

        public string? Street { get; set; }

        public string? Number { get; set; }

        public string? District { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? ZipCode { get; set; }

        public bool IsProspect { get; set; }

        public bool IsActive { get; set; }

        public string? Notes { get; set; }

        public long CreatedById { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
