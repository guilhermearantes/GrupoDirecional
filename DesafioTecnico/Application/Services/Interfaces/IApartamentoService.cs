using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Enums;

namespace DesafioTecnico.Application.Services.Interfaces
{
    /// <summary>
    /// Define operações de gerenciamento de apartamentos.
    /// </summary>
    public interface IApartamentoService
    {
        Task<IEnumerable<Apartamento>> GetAllAsync(CancellationToken ct = default);
        Task<Apartamento?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<(IEnumerable<Apartamento> Items, int Total)> GetPagedAsync(int page, int pageSize, StatusApartamento? status = null, CancellationToken ct = default);
        Task<Apartamento> CreateAsync(Apartamento apt, CancellationToken ct = default);
        Task UpdateAsync(Apartamento apt, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
