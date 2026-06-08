using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Enums;

namespace DesafioTecnico.Infrastructure.Repositories.Interfaces
{
    public interface IApartamentoRepository
    {
        Task<IEnumerable<Apartamento>> GetAllAsync(CancellationToken ct = default);
        Task<Apartamento?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<int> CountAsync(StatusApartamento? status = null, CancellationToken ct = default);
        Task<IEnumerable<Apartamento>> GetPagedAsync(int skip, int take, StatusApartamento? status = null, CancellationToken ct = default);
        Task AddAsync(Apartamento apt, CancellationToken ct = default);
        Task UpdateAsync(Apartamento apt, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
