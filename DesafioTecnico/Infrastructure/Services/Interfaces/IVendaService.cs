using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Services.Interfaces
{
    public interface IVendaService
    {
        Task<IEnumerable<Venda>> GetAllAsync(CancellationToken ct = default);
        Task<Venda?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Venda> CreateAsync(Venda venda, CancellationToken ct = default);
        Task UpdateAsync(Venda venda, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
