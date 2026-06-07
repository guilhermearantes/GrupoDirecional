using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Services.Interfaces
{
    public interface IReservaService
    {
        Task<IEnumerable<Reserva>> GetAllAsync(CancellationToken ct = default);
        Task<Reserva?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Reserva> CreateAsync(Reserva reserva, CancellationToken ct = default);
        Task ConfirmAsync(Guid id, CancellationToken ct = default);
        Task CancelAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Reserva reserva, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
