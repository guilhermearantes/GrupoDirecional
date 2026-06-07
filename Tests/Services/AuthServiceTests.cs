using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DesafioTecnico.Infrastructure.Data;

namespace Tests.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task Authenticate_ReturnsToken_WhenCredentialsValid()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestAuthDb")
                .Options;

            using var context = new AppDbContext(options);

            var user = new DesafioTecnico.Domain.Entities.Usuario
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret"),
                Role = "Admin"
            };

            context.Usuarios.Add(user);
            context.SaveChanges();

            var inMemorySettings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF",
                ["Jwt:Issuer"] = "tests",
                ["Jwt:Audience"] = "tests",
                ["Jwt:ExpiryMinutes"] = "60"
            };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

            var tokenGenerator = new DesafioTecnico.Infrastructure.Security.JwtTokenGenerator(configuration);
            var authService = new DesafioTecnico.Infrastructure.Services.AuthService(context, tokenGenerator);

            var result = await authService.AuthenticateAsync("admin", "secret");

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result!.Value.Token));
        }

        [Fact]
        public async Task Authenticate_ReturnsNull_WhenPasswordInvalid()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestAuthDb_InvalidPassword")
                .Options;

            using var context = new AppDbContext(options);

            var user = new DesafioTecnico.Domain.Entities.Usuario
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret"),
                Role = "Admin"
            };

            context.Usuarios.Add(user);
            context.SaveChanges();

            var inMemorySettings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF",
                ["Jwt:Issuer"] = "tests",
                ["Jwt:Audience"] = "tests",
                ["Jwt:ExpiryMinutes"] = "60"
            };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

            var tokenGenerator = new DesafioTecnico.Infrastructure.Security.JwtTokenGenerator(configuration);
            var authService = new DesafioTecnico.Infrastructure.Services.AuthService(context, tokenGenerator);

            var result = await authService.AuthenticateAsync("admin", "wrongpassword");
            Assert.Null(result);
        }

        [Fact]
        public async Task Authenticate_ReturnsNull_WhenUserNotFound()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestAuthDb_NoUser")
                .Options;

            using var context = new AppDbContext(options);

            var inMemorySettings = new Dictionary<string, string>
            {
                { "Jwt:Key", "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF" },
                { "Jwt:Issuer", "tests" },
                { "Jwt:Audience", "tests" },
                { "Jwt:ExpiryMinutes", "60" }
            };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

            var tokenGenerator = new DesafioTecnico.Infrastructure.Security.JwtTokenGenerator(configuration);
            var authService = new DesafioTecnico.Infrastructure.Services.AuthService(context, tokenGenerator);

            var result = await authService.AuthenticateAsync("nonexistent", "secret");
            Assert.Null(result);
        }
    }
}
