using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalgadosTrigo.Api.Entities.Catalog;

namespace SalgadosTrigo.Api.Data.Configurations
{
    public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
    {
        public void Configure(EntityTypeBuilder<Recipe> builder)
        {
            builder.ToTable("recipes");

            builder.HasIndex(recipe => recipe.FinishedProductId).IsUnique();

            builder.Property(recipe => recipe.YieldQuantity)
                .HasPrecision(14, 3)
                .HasDefaultValue(1m);

            builder.Property(recipe => recipe.UpdatedAt).HasDefaultValueSql("now()");

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(recipe => recipe.FinishedProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}