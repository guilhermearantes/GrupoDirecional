using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;

namespace DesafioTecnico.Infrastructure.Repositories
{
    public class VendaRepository : IVendaRepository
    {
        private readonly AppDbContext _context;

        public VendaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venda>> GetAllAsync()
        {
            return await _context.Vendas.AsNoTracking().ToListAsync();
        }

        public async Task<Venda?> GetByIdAsync(Guid id)
        {
            return await _context.Vendas.FindAsync(id);
        }

        public async Task AddAsync(Venda venda)
        {
            await _context.Vendas.AddAsync(venda);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Venda venda)
        {
            _context.Vendas.Update(venda);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Vendas.FindAsync(id);
            if (entity != null)
            {
                _context.Vendas.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
