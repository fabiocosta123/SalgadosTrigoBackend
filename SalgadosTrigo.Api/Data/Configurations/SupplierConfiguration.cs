using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalgadosTrigo.Api.Entities.Catalog;
using SalgadosTrigo.Api.Entities.Identity;

namespace SalgadosTrigo.Api.Data.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("suppliers");

            builder.HasIndex(supplier => supplier.Document).IsUnique();

            builder.Property(supplier => supplier.State)
                .HasMaxLength(2)
                .IsFixedLength();

            builder.Property(supplier => supplier.IsActive).HasDefaultValue(true);
            builder.Property(supplier => supplier.CreatedAt).HasDefaultValueSql("now()");

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(supplier => supplier.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}