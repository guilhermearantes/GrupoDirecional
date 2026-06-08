using Moq;
using Microsoft.AspNetCore.Mvc;
using DesafioTecnico.Api.Controllers;
using DesafioTecnico.Application.Services.Interfaces;
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

        [Fact]
        public async Task Login_DeveRetornarOkComToken_QuandoCredenciaisValidas()
        {
            var authMock = new Mock<IAuthService>();
            authMock.Setup(a => a.AuthenticateAsync("admin", "admin123", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(("jwt-token-aqui", 3600));

            var controller = new AuthController(authMock.Object);
            var res = await controller.Login(new LoginRequest { Username = "admin", Password = "admin123" }) as OkObjectResult;

            Assert.NotNull(res);
            var body = res.Value as LoginResponse;
            Assert.NotNull(body);
            Assert.Equal("jwt-token-aqui", body.Token);
            Assert.Equal(3600, body.ExpiresIn);
        }
    }
}
