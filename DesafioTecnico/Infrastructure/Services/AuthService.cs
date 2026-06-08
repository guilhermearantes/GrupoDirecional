using Microsoft.Extensions.Logging;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Security;
using DesafioTecnico.Infrastructure.Services.Interfaces;

namespace DesafioTecnico.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly JwtTokenGenerator _tokenGenerator;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUnitOfWork uow, JwtTokenGenerator tokenGenerator, ILogger<AuthService> logger)
        {
            _uow = uow;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
        }

        public async Task<(string Token, int ExpiresInSeconds)?> AuthenticateAsync(string username, string password, CancellationToken ct = default)
        {
            var user = await _uow.Usuarios.GetByUsernameAsync(username, ct);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Login falhou para '{Username}'", username);
                return null;
            }

            var token = _tokenGenerator.GenerateToken(user);
            var expiresInSeconds = _tokenGenerator.GetExpiryMinutes() * 60;
            _logger.LogInformation("Login bem-sucedido para '{Username}'", username);
            return (token, expiresInSeconds);
        }
    }
}
