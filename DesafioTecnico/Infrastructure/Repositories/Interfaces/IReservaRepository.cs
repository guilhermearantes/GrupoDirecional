using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Repositories.Interfaces
{
    public interface IReservaRepository
    {
        Task<IEnumerable<Reserva>> GetAllAsync(CancellationToken ct = default);
        Task<Reserva?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<int> CountAsync(CancellationToken ct = default);
        Task<IEnumerable<Reserva>> GetPagedAsync(int skip, int take, CancellationToken ct = default);
        Task AddAsync(Reserva reserva, CancellationToken ct = default);
        Task UpdateAsync(Reserva reserva, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
