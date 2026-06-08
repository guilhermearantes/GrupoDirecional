using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;

namespace DesafioTecnico.Infrastructure.Repositories
{
    public class VendaRepository : IVendaRepository
    {
        private readonly AppDbContext _context;

        public VendaRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Venda>> GetAllAsync(CancellationToken ct = default)
            => await _context.Vendas.AsNoTracking().ToListAsync(ct);

        public Task<Venda?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _context.Vendas.FindAsync([id], ct).AsTask();

        public async Task AddAsync(Venda venda, CancellationToken ct = default)
            => await _context.Vendas.AddAsync(venda, ct);

        public Task UpdateAsync(Venda venda, CancellationToken ct = default)
        {
            _context.Vendas.Update(venda);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _context.Vendas.FindAsync([id], ct);
            if (entity != null)
                _context.Vendas.Remove(entity);
        }
    }
}
