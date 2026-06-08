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
                    ["Jwt:Key"]           = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF",
                    ["Jwt:Issuer"]        = "tests",
                    ["Jwt:Audience"]      = "tests",
                    ["Jwt:ExpiryMinutes"] = "60"
                })
                .Build();

            var uow = new UnitOfWork(context);
            var tokenGenerator = new JwtTokenGenerator(config);
            return new AuthService(uow, tokenGenerator, NullLogger<AuthService>.Instance);
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

        [Theory]
        [InlineData("admin",       "wrongpassword")] // usuário existe, senha errada
        [InlineData("nonexistent", "secret")]        // usuário não encontrado
        public async Task Autenticar_DeveRetornarNull_QuandoCredenciaisInvalidas(string username, string password)
        {
            var service = BuildSut($"TestAuthDb_Invalid_{username}", out var context);
            // Sempre semeia o usuário "admin"; o caso "nonexistent" simplesmente não o encontrará
            context.Usuarios.Add(new DesafioTecnico.Domain.Entities.Usuario
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret"),
                Role = "Admin"
            });
            context.SaveChanges();

            var result = await service.AuthenticateAsync(username, password);

            Assert.Null(result);
        }
    }
}
