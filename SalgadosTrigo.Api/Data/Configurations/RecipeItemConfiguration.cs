using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalgadosTrigo.Api.Entities.Catalog;

namespace SalgadosTrigo.Api.Data.Configurations
{
    public class RecipeItemConfiguration : IEntityTypeConfiguration<RecipeItem>
    {
        public void Configure(EntityTypeBuilder<RecipeItem> builder)
        {
            builder.ToTable("recipe_items");

            builder.HasIndex(item => new { item.RecipeId, item.RawMaterialId }).IsUnique();

            builder.Property(item => item.Quantity).HasPrecision(14, 3);

            builder.HasOne<Recipe>()
                .WithMany()
                .HasForeignKey(item => item.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(item => item.RawMaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}