using Xunit;
using Moq;
using Server.Controllers;
using Server.Services.Interfaces;
using Server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UserControllerTests
{
    [Fact]
    public async Task GetUsers_ReturnsOkResult()
    {
        var mockService = new Mock<IUserService>();
        mockService.Setup(s => s.GetUsersAsync()).ReturnsAsync(new List<User>());
        var controller = new UsersController(mockService.Object);

        var result = await controller.GetUsers();

        Assert.IsType<OkObjectResult>(result.Result);
    }
}