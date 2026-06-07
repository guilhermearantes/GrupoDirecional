using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Repositories.Interfaces
{
    public interface IApartamentoRepository
    {
        Task<IEnumerable<Apartamento>> GetAllAsync(CancellationToken ct = default);
        Task<Apartamento?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Apartamento apt, CancellationToken ct = default);
        Task UpdateAsync(Apartamento apt, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<bool> IsAvailableAsync(Guid apartamentoId, CancellationToken ct = default);
    }
}
