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

        public Task<IEnumerable<Apartamento>> GetAllAsync(CancellationToken ct = default)
            => _context.Apartamentos.AsNoTracking().ToListAsync(ct).ContinueWith(t => (IEnumerable<Apartamento>)t.Result, ct);

        public Task<Apartamento?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _context.Apartamentos.FindAsync(new object[] { id }, ct).AsTask();

        public async Task AddAsync(Apartamento apt, CancellationToken ct = default)
        {
            await _context.Apartamentos.AddAsync(apt, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Apartamento apt, CancellationToken ct = default)
        {
            _context.Apartamentos.Update(apt);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _context.Apartamentos.FindAsync(new object[] { id }, ct);
            if (entity != null)
            {
                _context.Apartamentos.Remove(entity);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<bool> IsAvailableAsync(Guid apartamentoId, CancellationToken ct = default)
        {
            var apt = await _context.Apartamentos.FindAsync(new object[] { apartamentoId }, ct);
            return apt?.Status == Domain.Enums.StatusApartamento.Disponivel;
        }
    }
}
