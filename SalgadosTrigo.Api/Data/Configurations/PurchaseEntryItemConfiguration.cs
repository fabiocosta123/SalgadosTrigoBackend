using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalgadosTrigo.Api.Entities.Catalog;
using SalgadosTrigo.Api.Entities.Stock;

namespace SalgadosTrigo.Api.Data.Configurations
{
    public class PurchaseEntryItemConfiguration : IEntityTypeConfiguration<PurchaseEntryItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseEntryItem> builder)
        {
            builder.ToTable("purchase_entry_items");

            builder.Property(item => item.Quantity).HasPrecision(14, 3);
            builder.Property(item => item.UnitCost).HasPrecision(14, 2);
            builder.Property(item => item.LineTotal).HasPrecision(14, 2);

            builder.HasOne<PurchaseEntry>()
                .WithMany()
                .HasForeignKey(item => item.PurchaseEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}