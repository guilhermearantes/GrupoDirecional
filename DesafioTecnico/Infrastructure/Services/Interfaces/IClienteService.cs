using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Define operações de gerenciamento de clientes.
    /// </summary>
    public interface IClienteService
    {
        Task<IEnumerable<Cliente>> GetAllAsync(CancellationToken ct = default);
        Task<Cliente?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Cliente> CreateAsync(Cliente cliente, CancellationToken ct = default);
        Task UpdateAsync(Cliente cliente, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
