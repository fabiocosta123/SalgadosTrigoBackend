using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalgadosTrigo.Api.Entities.Catalog;
using SalgadosTrigo.Api.Entities.Stock;

namespace SalgadosTrigo.Api.Data.Configurations
{
    public class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
    {
        public void Configure(EntityTypeBuilder<StockBalance> builder)
        {
            builder.ToTable("stock_balances", table => 
                table.HasCheckConstraint("CK_stock_balances_Quantity", "\"Quantity\" >= 0"));
            
            builder.HasIndex(balance => balance.ProductId).IsUnique();
            builder.Property(balance => balance.Quantity).HasPrecision(14, 3);
            builder.Property(balance => balance.UpdatedAt).HasDefaultValueSql("now()");

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(balance => balance.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}