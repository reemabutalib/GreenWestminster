using Xunit;
using Moq;
using Server.Controllers;
using Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ChallengesControllerTests
{
    [Fact]
    public async Task GetChallenges_ReturnsOkResult()
    {
        var mockService = new Mock<IChallengesService>();
        mockService.Setup(s => s.GetChallengesAsync()).ReturnsAsync(new List<object>());
        var controller = new ChallengesController(mockService.Object, null);

        var result = await controller.GetChallenges();

        Assert.IsType<OkObjectResult>(result.Result);
    }
}