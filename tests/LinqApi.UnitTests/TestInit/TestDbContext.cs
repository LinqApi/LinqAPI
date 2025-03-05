using Microsoft.EntityFrameworkCore;

namespace LinqApi.UnitTests.TestInit
{
    // Test DbContext ve Entity örnekleri:
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestEntity> TestEntities { get; set; }
    }
}
