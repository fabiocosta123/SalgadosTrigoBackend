using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Entities.Catalog
{
    public class Customer
    {
        public long Id { get; set; }


        [StringLength(160)]
        public required string Name { get; set; }

        [StringLength(160)]
        public string? TradeName { get; set; }

        [StringLength(20)]
        public string? Document { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(180)]
        public string? Email { get; set; }

        [StringLength(20)]
        public required string Segment { get; set; }

        [StringLength(80)]
        public string? City { get; set; }

        [StringLength(160)]
        public string? Street { get; set; }

        [StringLength(20)]
        public string? Number { get; set; }

        [StringLength(80)]
        public string? District { get; set; }        

        [StringLength(2)]
        public string? State { get; set; }

        [StringLength(9)]
        public string? ZipCode { get; set; }

        public bool IsProspect { get; set; }
        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }

        public long CreatedById { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

    }
}
