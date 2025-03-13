using Microsoft.EntityFrameworkCore;

namespace LinqApi.Demo.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

        public DbSet<MyEntity> MyEntities { get; set; } // BaseEntity<long> türetilmiş bir entity
        public DbSet<MyTestEntity> MyTestEntities { get; set; } // BaseEntity<long> türetilmiş bir entity
    }
}