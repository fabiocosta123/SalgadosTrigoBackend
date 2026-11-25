using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalgadosTrigo.Api.Entities.Catalog;
using SalgadosTrigo.Api.Entities.Identity;
using SalgadosTrigo.Api.Entities.Stock;

namespace SalgadosTrigo.Api.Data.Configurations
{
    public class PurchaseEntryConfiguration : IEntityTypeConfiguration<PurchaseEntry>
    {
        public void Configure(EntityTypeBuilder<PurchaseEntry> builder)
        {
            builder.ToTable("purchase_entries");

            builder.Property(entry => entry.EntryDate).HasColumnType("date");
            builder.Property(entry => entry.TotalAmount).HasPrecision(14, 2);

            builder.Property(entry => entry.CreatedAt).HasDefaultValueSql("now()");

            builder.HasOne<Supplier>()
                .WithMany()
                .HasForeignKey(entry => entry.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(entry => entry.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}