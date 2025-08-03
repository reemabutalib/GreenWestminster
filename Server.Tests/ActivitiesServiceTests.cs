using Xunit;
using Moq;
using Server.Services.Implementations;
using Server.Services.Interfaces;
using Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ActivitiesServiceTests
{
    [Fact]
    public async Task GetAllActivitiesAsync_ReturnsList()
    {
        var mockActivityRepo = new Mock<ISustainableActivityRepository>();
        mockActivityRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SustainableActivity>());
        var mockCompletionRepo = new Mock<IActivityCompletionRepository>();
        var service = new ActivitiesService(mockActivityRepo.Object, mockCompletionRepo.Object);

        var result = await service.GetAllActivitiesAsync();

        Assert.NotNull(result);
        Assert.IsType<List<SustainableActivity>>(result);
    }
}