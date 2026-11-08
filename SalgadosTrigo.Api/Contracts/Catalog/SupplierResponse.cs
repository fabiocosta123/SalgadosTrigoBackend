namespace SalgadosTrigo.Api.Contracts.Catalog
{
    public class SupplierResponse
    {
        public long Id { get; set; }

        public required string Name { get; set; }

        public string? Document { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public bool IsActive { get; set; }

        public long CreatedById { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
