using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalgadosTrigo.Api.Entities.Catalog;
using SalgadosTrigo.Api.Entities.Identity;
using SalgadosTrigo.Api.Entities.Stock;

namespace SalgadosTrigo.Api.Data.Configurations
{
    public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(EntityTypeBuilder<StockMovement> builder)
        {
            builder.ToTable("stock_movements", table =>
                table.HasCheckConstraint("CK_stock_movements_Quantity", "\"Quantity\" > 0"));

            builder.Property(movement => movement.Quantity).HasPrecision(14, 3);
            builder.Property(movement => movement.UnitCost).HasPrecision(14, 2);
            builder.Property(movement => movement.BalanceAfter).HasPrecision(14, 3);

            builder.Property(movement => movement.CreatedAt).HasDefaultValueSql("now()");

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(movement => movement.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(movement => movement.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}