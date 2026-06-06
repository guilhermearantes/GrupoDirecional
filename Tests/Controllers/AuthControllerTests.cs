using Moq;
using Microsoft.AspNetCore.Mvc;
using DesafioTecnico.Api.Controllers;
using DesafioTecnico.Infraestructure.Services.Interfaces;
using DesafioTecnico.Api.DTOs;

namespace Tests.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var authMock = new Mock<IAuthService>();
            authMock.Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(((string Token, int ExpiresInSeconds)?)null);

            var controller = new AuthController(authMock.Object);

            var req = new LoginRequest { Username = "u", Password = "p" };
            var res = await controller.Login(req) as UnauthorizedResult;

            Assert.NotNull(res);
        }
    }
}
