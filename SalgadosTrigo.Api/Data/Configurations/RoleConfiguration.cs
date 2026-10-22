using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalgadosTrigo.Api.Entities.Identity;

namespace SalgadosTrigo.Api.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasIndex(role => role.Code).IsUnique();

        builder.HasData(
            new Role { Id = 1, Code = "administrador", Name = "Administrador"},
            new Role { Id = 2, Code = "gerente", Name = "Gerente" },
            new Role { Id = 3, Code = "funcionario", Name = "Funcionário" },
            new Role { Id = 4, Code = "vendedor", Name = "Vendedor" },
            new Role { Id = 5, Code = "financeiro", Name = "Financeiro" }
        );
    }
}
