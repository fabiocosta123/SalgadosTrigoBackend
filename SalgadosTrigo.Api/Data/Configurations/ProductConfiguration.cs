using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalgadosTrigo.Api.Entities.Catalog;

namespace SalgadosTrigo.Api.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");

            builder.HasIndex(product => product.Code).IsUnique();

            builder.Property(product => product.CurrentSalePrice).HasPrecision(14, 2);

            builder.Property(product => product.IsActive).HasDefaultValue(true);
            builder.Property(product => product.CreatedAt).HasDefaultValueSql("now()");
        }
    }
}