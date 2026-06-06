using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Infraestructure.Data;
using DesafioTecnico.Infraestructure.Security;

namespace DesafioTecnico.Infraestructure.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenGenerator _tokenGenerator;

        public AuthService(AppDbContext context, JwtTokenGenerator tokenGenerator)
        {
            _context = context;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<string?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return null;

            // Verifica hash usando BCrypt
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

            return _tokenGenerator.GenerateToken(user);
        }
    }
}
