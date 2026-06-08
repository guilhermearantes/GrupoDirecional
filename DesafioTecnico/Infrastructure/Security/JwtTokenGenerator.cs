using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Security
{
    /// <summary>
    /// Gera tokens JWT assinados com HS256 usando as configurações <c>Jwt:Key</c>, <c>Jwt:Issuer</c>, <c>Jwt:Audience</c> e <c>Jwt:ExpiryMinutes</c>.
    /// </summary>
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _config;

        public JwtTokenGenerator(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>Retorna o tempo de expiração em minutos configurado em <c>Jwt:ExpiryMinutes</c> (padrão: 60).</summary>
        public int GetExpiryMinutes() => int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");

        /// <summary>Gera um token JWT para o usuário informado com claims de Sub, NameIdentifier e Role.</summary>
        public string GenerateToken(Usuario user)
        {
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
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
