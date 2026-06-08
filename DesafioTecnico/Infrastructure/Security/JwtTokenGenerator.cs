using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Security
{
    /// <summary>
    /// Gera tokens JWT assinados com HS256 usando <see cref="JwtSettings"/> injetado via IOptions.
    /// </summary>
    public class JwtTokenGenerator
    {
        private readonly JwtSettings _settings;

        public JwtTokenGenerator(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        /// <summary>Retorna o tempo de expiração em minutos configurado em <c>Jwt:ExpiryMinutes</c> (padrão: 60).</summary>
        public int GetExpiryMinutes() => _settings.ExpiryMinutes;

        /// <summary>Gera um token JWT para o usuário informado com claims de Sub, NameIdentifier e Role.</summary>
        public string GenerateToken(Usuario user)
        {
            var key = !string.IsNullOrEmpty(_settings.Key)
                ? _settings.Key
                : throw new InvalidOperationException("Jwt:Key is not configured");
            var issuer = _settings.Issuer;
            var audience = _settings.Audience;
            var expiryMinutes = GetExpiryMinutes();

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
