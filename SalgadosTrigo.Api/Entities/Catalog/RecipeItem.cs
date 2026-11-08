namespace SalgadosTrigo.Api.Entities.Catalog
{
    public class RecipeItem
    {
        public long Id { get; set; }

        public long RecipeId { get; set; }

        public long RawMaterialId { get; set; }

        public decimal Quantity { get; set; }
    }
}