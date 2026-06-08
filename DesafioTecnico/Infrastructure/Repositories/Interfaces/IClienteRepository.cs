using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Repositories.Interfaces
{
    public interface IClienteRepository
    {
        Task<IEnumerable<Cliente>> GetAllAsync(CancellationToken ct = default);
        Task<Cliente?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<int> CountAsync(CancellationToken ct = default);
        Task<IEnumerable<Cliente>> GetPagedAsync(int skip, int take, CancellationToken ct = default);
        Task AddAsync(Cliente cliente, CancellationToken ct = default);
        Task UpdateAsync(Cliente cliente, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
