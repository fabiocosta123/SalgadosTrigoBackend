using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Contracts.Catalog
{
    public class ProductRequest
    {
        [Required(ErrorMessage = "Informe o nome do produto.")]
        [StringLength(160)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Informe o tipo do produto: materia_prima ou produto_acabado.")]
        [StringLength(20)]
        public required string Kind { get; set; }

        [Required(ErrorMessage = "Informe a unidade: un, kg, g ou l.")]
        [StringLength(10)]
        public required string Unit { get; set; }

        [StringLength(40)]
        public string? Code { get; set; }

        [StringLength(80)]
        public string? Category { get; set; }

        public decimal? CurrentSalePrice { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
