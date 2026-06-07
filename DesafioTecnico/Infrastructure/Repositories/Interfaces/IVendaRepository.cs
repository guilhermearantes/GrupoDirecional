using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Repositories.Interfaces
{
    public interface IVendaRepository
    {
        Task<IEnumerable<Venda>> GetAllAsync(CancellationToken ct = default);
        Task<Venda?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Venda venda, CancellationToken ct = default);
        Task UpdateAsync(Venda venda, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
