using LinqApi.Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace LinqApi.Demo.Data
{
    public class DemoDbContext : DbContext
    {
        public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options) { }

        public DbSet<ProductEntity> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Bogus data seed
            modelBuilder.Entity<ProductEntity>().HasData(
                new ProductEntity { Id = 1, Name = "Product A", Price = 10.99m },
                new ProductEntity { Id = 2, Name = "Product B", Price = 20.50m },
                new ProductEntity { Id = 3, Name = "Product C", Price = 15.75m }
            );
        }
    }
}
