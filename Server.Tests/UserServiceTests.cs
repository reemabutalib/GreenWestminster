using Xunit;
using Moq;
using Server.Services.Implementations;
using Server.Services.Interfaces;
using Server.Models;
using System.Threading.Tasks;
using Server.Repositories.Interfaces;

public class UserServiceTests
{
    [Fact]
    public async Task GetUserAsync_ReturnsUser()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
        var service = new UserService(mockRepo.Object, null, null, null);

        var user = await service.GetUserAsync(1);

        Assert.NotNull(user);
        Assert.Equal(1, user.Id);
    }
}