using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Contracts.Catalog
{
    public class CustomerRequest
    {
        [Required(ErrorMessage = "Informe o nome do cliente.")]
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

        [Required(ErrorMessage = "Informe o segmento: atacado ou varejo.")]
        [StringLength(20)]
        public required string Segment { get; set; }

        [StringLength(160)]
        public string? Street { get; set; }

        [StringLength(20)]
        public string? Number { get; set; }

        [StringLength(80)]
        public string? District { get; set; }

        [StringLength(80)]
        public string? City { get; set; }

        [StringLength(2)]
        public string? State { get; set; }

        [StringLength(9)]
        public string? ZipCode { get; set; }

        public bool IsProspect { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
