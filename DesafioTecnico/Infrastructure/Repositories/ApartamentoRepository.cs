using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;

namespace DesafioTecnico.Infrastructure.Repositories
{
    public class ApartamentoRepository : IApartamentoRepository
    {
        private readonly AppDbContext _context;

        public ApartamentoRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Apartamento>> GetAllAsync(CancellationToken ct = default)
            => await _context.Apartamentos.AsNoTracking().ToListAsync(ct);

        public Task<Apartamento?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _context.Apartamentos.FindAsync([id], ct).AsTask();

        public async Task AddAsync(Apartamento apt, CancellationToken ct = default)
            => await _context.Apartamentos.AddAsync(apt, ct);

        public Task UpdateAsync(Apartamento apt, CancellationToken ct = default)
        {
            _context.Apartamentos.Update(apt);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _context.Apartamentos.FindAsync([id], ct);
            if (entity != null)
                _context.Apartamentos.Remove(entity);
        }
    }
}
