using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalgadosTrigo.Api.Entities.Catalog;
using SalgadosTrigo.Api.Entities.Identity;

namespace SalgadosTrigo.Api.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("customers");

            builder.HasIndex(customer => customer.Document).IsUnique();

            builder.Property(customer => customer.State)
                .HasMaxLength(2)
                .IsFixedLength();

            builder.Property(customer => customer.IsProspect).HasDefaultValue(false);
            builder.Property(customer => customer.IsActive).HasDefaultValue(true);
            builder.Property(customer => customer.CreatedAt).HasDefaultValueSql("now()");

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(customer => customer.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
