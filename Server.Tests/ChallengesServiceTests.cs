using Xunit;
using Moq;
using Server.Services.Implementations;
using Server.Services.Interfaces;
using Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ChallengesServiceTests
{
    [Fact]
    public async Task GetChallengesAsync_ReturnsList()
    {
        var mockRepo = new Mock<IChallengeRepository>();
        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Challenge>());
        var service = new ChallengesService(mockRepo.Object);

        var result = await service.GetChallengesAsync();

        Assert.NotNull(result);
    }
}