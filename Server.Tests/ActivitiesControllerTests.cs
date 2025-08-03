using Xunit;
using Moq;
using Server.Controllers;
using Server.Services.Interfaces;
using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ActivitiesControllerTests
{
    [Fact]
    public async Task GetActivities_ReturnsOkResult()
    {
        var mockService = new Mock<IActivitiesService>();
        mockService.Setup(s => s.GetAllActivitiesAsync()).ReturnsAsync(new List<SustainableActivity>());
        var mockLogger = new Mock<ILogger<ActivitiesController>>();
        var controller = new ActivitiesController(mockService.Object, mockLogger.Object);

        var result = await controller.GetActivities();

        Assert.IsType<OkObjectResult>(result.Result);
    }
}