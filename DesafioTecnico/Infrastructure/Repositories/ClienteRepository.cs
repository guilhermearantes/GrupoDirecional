using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;

namespace DesafioTecnico.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppDbContext _context;

        public ClienteRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Cliente>> GetAllAsync(CancellationToken ct = default)
            => await _context.Clientes.AsNoTracking().ToListAsync(ct);

        public Task<Cliente?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _context.Clientes.FindAsync([id], ct).AsTask();

        public async Task AddAsync(Cliente cliente, CancellationToken ct = default)
            => await _context.Clientes.AddAsync(cliente, ct);

        public Task UpdateAsync(Cliente cliente, CancellationToken ct = default)
        {
            _context.Clientes.Update(cliente);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _context.Clientes.FindAsync([id], ct);
            if (entity != null)
                _context.Clientes.Remove(entity);
        }
    }
}
