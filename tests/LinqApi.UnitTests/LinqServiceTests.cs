using AutoMapper;
using LinqApi.Model;
using LinqApi.Repository;
using LinqApi.Service;
using LinqApi.UnitTests.TestInit;
using Moq;

namespace LinqAPI.Tests.ServiceTests
{
    public class LinqServiceTests
    {
        private readonly Mock<ILinqRepository<TestEntity, int>> _mockRepository;
        private readonly IMapper _mapper;
        private readonly LinqService<TestEntity, TestDto, int> _service;

        public LinqServiceTests()
        {
            _mockRepository = new Mock<ILinqRepository<TestEntity, int>>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TestDto, TestEntity>().ReverseMap();
            });
            _mapper = config.CreateMapper();
            _service = new LinqService<TestEntity, TestDto, int>(_mockRepository.Object, _mapper);
        }

        [Fact]
        public async Task InsertAsync_Should_CallRepositoryAndReturnMappedDto()
        {
            // Arrange
            var dto = new TestDto { Id = 0, Name = "Test" };
            var entity = new TestEntity { Id = 1, Name = "Test" };
            _mockRepository.Setup(r => r.InsertAsync(It.IsAny<TestEntity>(), default))
                           .ReturnsAsync(entity);

            // Act
            var result = await _service.InsertAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test", result.Name);
        }

        // Update, Delete, GetById vb. metotlar için benzer testler eklenebilir.
    }

    public class TestDto : BaseDto<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
