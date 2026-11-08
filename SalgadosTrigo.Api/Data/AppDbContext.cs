using Microsoft.EntityFrameworkCore;
using SalgadosTrigo.Api.Entities.Catalog;
using SalgadosTrigo.Api.Entities.Identity;

namespace SalgadosTrigo.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<RecipeItem> RecipeItems => Set<RecipeItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
