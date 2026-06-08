using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;

namespace DesafioTecnico.Infrastructure.Repositories
{
    public class ReservaRepository : IReservaRepository
    {
        private readonly AppDbContext _context;

        public ReservaRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Reserva>> GetAllAsync(CancellationToken ct = default)
            => await _context.Reservas.AsNoTracking().ToListAsync(ct);

        public Task<Reserva?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _context.Reservas.FindAsync([id], ct).AsTask();

        public Task<int> CountAsync(CancellationToken ct = default)
            => _context.Reservas.CountAsync(ct);

        public async Task<IEnumerable<Reserva>> GetPagedAsync(int skip, int take, CancellationToken ct = default)
            => await _context.Reservas.AsNoTracking().Skip(skip).Take(take).ToListAsync(ct);

        public async Task AddAsync(Reserva reserva, CancellationToken ct = default)
            => await _context.Reservas.AddAsync(reserva, ct);

        public Task UpdateAsync(Reserva reserva, CancellationToken ct = default)
        {
            _context.Reservas.Update(reserva);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _context.Reservas.FindAsync([id], ct);
            if (entity != null)
                _context.Reservas.Remove(entity);
        }
    }
}
