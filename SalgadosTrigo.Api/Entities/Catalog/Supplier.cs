using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Entities.Catalog
{
    public class Supplier
    {
        public long Id { get; set; }

        [StringLength(160)]
        public required string Name { get; set; }

        [StringLength(20)]
        public string? Document { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(180)]
        public string? Email { get; set; }

        [StringLength(80)]
        public string? City { get; set; }

        [StringLength(2)]
        public string? State { get; set; }

        public bool IsActive { get; set; } = true;
        public long CreatedById { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
