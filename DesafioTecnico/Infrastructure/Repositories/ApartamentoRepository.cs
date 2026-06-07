using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;

namespace DesafioTecnico.Infrastructure.Repositories
{
    public class ApartamentoRepository : IApartamentoRepository
    {
        private readonly AppDbContext _context;

        public ApartamentoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Apartamento>> GetAllAsync()
        {
            return await _context.Apartamentos.AsNoTracking().ToListAsync();
        }

        public async Task<Apartamento?> GetByIdAsync(Guid id)
        {
            return await _context.Apartamentos.FindAsync(id);
        }

        public async Task AddAsync(Apartamento apt)
        {
            await _context.Apartamentos.AddAsync(apt);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Apartamento apt)
        {
            _context.Apartamentos.Update(apt);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Apartamentos.FindAsync(id);
            if (entity != null)
            {
                _context.Apartamentos.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsAvailableAsync(Guid apartamentoId)
        {
            var apt = await _context.Apartamentos.FindAsync(apartamentoId);
            if (apt == null) return false;
            return apt.Status == Domain.Enums.StatusApartamento.Disponivel;
        }
    }
}
