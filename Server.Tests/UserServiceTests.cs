using Xunit;
using Moq;
using Server.Services.Implementations;
using Server.Services.Interfaces;
using Server.Models;
using System.Threading.Tasks;
using Server.Repositories.Interfaces;
using Server.Data;
using Microsoft.EntityFrameworkCore;

public class UserServiceTests
{
    [Fact]
    public async Task GetUserAsync_ReturnsUser()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1 });

        var mockActivityCompletionRepo = new Mock<IActivityCompletionRepository>();
        var mockChallengeRepo = new Mock<IChallengeRepository>();
        var mockSustainableActivityRepo = new Mock<ISustainableActivityRepository>();

        // Use in-memory AppDbContext instead of mocking
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "UserServiceTestDb")
            .Options;
        var dbContext = new AppDbContext(options);

        var service = new UserService(
            mockRepo.Object,
            mockActivityCompletionRepo.Object,
            mockChallengeRepo.Object,
            mockSustainableActivityRepo.Object,
            dbContext // Pass the in-memory context
        );

        var user = await service.GetUserAsync(1);

        Assert.NotNull(user);
        Assert.Equal(1, user.Id);
    }
}