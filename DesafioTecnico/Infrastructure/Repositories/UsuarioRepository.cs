using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;

namespace DesafioTecnico.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context) => _context = context;

        public Task<Usuario?> GetByUsernameAsync(string username, CancellationToken ct = default)
            => _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username, ct);
    }
}
