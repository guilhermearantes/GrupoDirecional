using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Security;
using DesafioTecnico.Infrastructure.Services;

namespace Tests.Services
{
    public class AuthServiceTests
    {
        private static AuthService BuildSut(string dbName, out AppDbContext context)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            context = new AppDbContext(options);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"]          = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF",
                    ["Jwt:Issuer"]       = "tests",
                    ["Jwt:Audience"]     = "tests",
                    ["Jwt:ExpiryMinutes"] = "60"
                })
                .Build();

            var tokenGenerator = new JwtTokenGenerator(config);
            return new AuthService(context, tokenGenerator, NullLogger<AuthService>.Instance);
        }

        [Fact]
        public async Task Autenticar_DeveRetornarToken_QuandoCredenciaisValidas()
        {
            var service = BuildSut("TestAuthDb_Valid", out var context);
            context.Usuarios.Add(new DesafioTecnico.Domain.Entities.Usuario
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret"),
                Role = "Admin"
            });
            context.SaveChanges();

            var result = await service.AuthenticateAsync("admin", "secret");

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result!.Value.Token));
        }

        [Fact]
        public async Task Autenticar_DeveRetornarNull_QuandoSenhaInvalida()
        {
            var service = BuildSut("TestAuthDb_InvalidPassword", out var context);
            context.Usuarios.Add(new DesafioTecnico.Domain.Entities.Usuario
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret"),
                Role = "Admin"
            });
            context.SaveChanges();

            var result = await service.AuthenticateAsync("admin", "wrongpassword");

            Assert.Null(result);
        }

        [Fact]
        public async Task Autenticar_DeveRetornarNull_QuandoUsuarioNaoEncontrado()
        {
            var service = BuildSut("TestAuthDb_NoUser", out _);

            var result = await service.AuthenticateAsync("nonexistent", "secret");

            Assert.Null(result);
        }
    }
}
