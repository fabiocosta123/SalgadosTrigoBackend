using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalgadosTrigo.Api.Entities.Identity;

namespace SalgadosTrigo.Api.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");
            builder.HasIndex(user => user.Email).IsUnique();
            builder.Property(user => user.IsActive).HasDefaultValue(true);
            builder.Property(user => user.CreatedAt).HasDefaultValueSql("now()");

            builder.HasOne<Role>()
                .WithMany()
                .HasForeignKey(user => user.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
