using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infraestructure.Data;
using DesafioTecnico.Infraestructure.Repositories.Interfaces;

namespace DesafioTecnico.Infraestructure.Repositories
{
    public class ReservaRepository : IReservaRepository
    {
        private readonly AppDbContext _context;

        public ReservaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reserva>> GetAllAsync()
        {
            return await _context.Reservas.AsNoTracking().ToListAsync();
        }

        public async Task<Reserva?> GetByIdAsync(Guid id)
        {
            return await _context.Reservas.FindAsync(id);
        }

        public async Task AddAsync(Reserva reserva)
        {
            await _context.Reservas.AddAsync(reserva);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Reserva reserva)
        {
            _context.Reservas.Update(reserva);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Reservas.FindAsync(id);
            if (entity != null)
            {
                _context.Reservas.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
