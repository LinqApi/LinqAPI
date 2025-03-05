using LinqApi.Controller;
using LinqApi.Service;
using LinqApi.UnitTests.TestInit;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LinqAPI.Tests.ControllerTests
{
    public class LinqControllerTests
    {
        private readonly Mock<ILinqService<TestEntity, TestDto, int>> _mockService;
        private readonly LinqController<TestEntity, TestDto, int> _controller;

        public LinqControllerTests()
        {
            _mockService = new Mock<ILinqService<TestEntity, TestDto, int>>();
            _controller = new LinqController<TestEntity, TestDto, int>(_mockService.Object);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenEntityFound()
        {
            // Arrange
            var testDto = new TestDto { Id = 1, Name = "Test" };
            _mockService.Setup(s => s.GetByIdAsync(1, default))
                        .ReturnsAsync(testDto);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(testDto, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenEntityNotFound()
        {
            // Arrange
            _mockService.Setup(s => s.GetByIdAsync(1, default))
                        .ReturnsAsync((TestDto)null);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // Create, Update, Delete, Filter gibi diðer metotlar için benzer testler eklenebilir.
    }
}
