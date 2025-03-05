using LinqApi.Repository;
using LinqApi.UnitTests.TestInit;
using Microsoft.EntityFrameworkCore;

namespace LinqAPI.Tests.RepositoryTests
{
    public class LinqRepositoryTests
    {
        private DbContextOptions<TestDbContext> _options;

        public LinqRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }

        [Fact]
        public async Task InsertAsync_InsertsEntitySuccessfully()
        {
            // Arrange
            using var context = new TestDbContext(_options);
            var repository = new LinqRepository<TestDbContext, TestEntity, int>(context);
            var entity = new TestEntity { Id = 1, Name = "Test" };

            // Act
            var inserted = await repository.InsertAsync(entity);
            var retrieved = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("Test", retrieved.Name);
        }

        // Diğer metotlar (Update, Delete, GetAll vb.) için benzer testler eklenebilir.
    }
}
