using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Services.Interfaces
{
    public interface IReservaService
    {
        Task<IEnumerable<Reserva>> GetAllAsync();
        Task<Reserva?> GetByIdAsync(Guid id);
        Task<Reserva> CreateAsync(Reserva reserva);
        Task ConfirmAsync(Guid id);
        Task CancelAsync(Guid id);
        Task UpdateAsync(Reserva reserva);
        Task DeleteAsync(Guid id);
    }
}
