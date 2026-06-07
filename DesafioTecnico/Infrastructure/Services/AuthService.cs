using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Security;

namespace DesafioTecnico.Infrastructure.Services
{
    public class AuthService : DesafioTecnico.Infrastructure.Services.Interfaces.IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenGenerator _tokenGenerator;

        public AuthService(AppDbContext context, JwtTokenGenerator tokenGenerator)
        {
            _context = context;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<(string Token, int ExpiresInSeconds)?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

            var token = _tokenGenerator.GenerateToken(user);
            var expiresInSeconds = _tokenGenerator.GetExpiryMinutes() * 60;
            return (token, expiresInSeconds);
        }
    }
}
