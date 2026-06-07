using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Services.Interfaces
{
    public interface IApartamentoService
    {
        Task<IEnumerable<Apartamento>> GetAllAsync(CancellationToken ct = default);
        Task<Apartamento?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Apartamento> CreateAsync(Apartamento apt, CancellationToken ct = default);
        Task UpdateAsync(Apartamento apt, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
