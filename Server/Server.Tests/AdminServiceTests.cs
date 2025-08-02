using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Server.Services.Implementations;
using Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace Server.Tests
{
    public class AdminServiceTests
    {
        private readonly AdminService _adminService;
        private readonly AppDbContext _context;
        private readonly Mock<ILogger<AdminService>> _mockLogger;

        public AdminServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
            _mockLogger = new Mock<ILogger<AdminService>>();
            _adminService = new AdminService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task GetUserStatsAsync_ReturnsObject()
        {
            // Arrange (optionally seed _context with test data)

            // Act
            var result = await _adminService.GetUserStatsAsync();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetActivityCompletionsAsync_ReturnsObject()
        {
            // Arrange
            DateTime start = DateTime.UtcNow.AddDays(-7);
            DateTime end = DateTime.UtcNow;

            // Act
            var result = await _adminService.GetActivityCompletionsAsync(start, end);

            // Assert
            Assert.NotNull(result);
        }
    }
}