using Moq;
using Microsoft.AspNetCore.Mvc;
using DesafioTecnico.Api.Controllers;
using DesafioTecnico.Infrastructure.Services.Interfaces;
using DesafioTecnico.Api.DTOs;

namespace Tests.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Login_DeveRetornarUnauthorized_QuandoCredenciaisInvalidas()
        {
            var authMock = new Mock<IAuthService>();
            authMock.Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(((string Token, int ExpiresInSeconds)?)null);

            var controller = new AuthController(authMock.Object);
            var res = await controller.Login(new LoginRequest { Username = "u", Password = "p" }) as UnauthorizedResult;

            Assert.NotNull(res);
        }
    }
}
