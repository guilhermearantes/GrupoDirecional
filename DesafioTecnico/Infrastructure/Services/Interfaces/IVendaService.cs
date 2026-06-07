using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Define operações relacionadas ao gerenciamento de vendas diretas de apartamentos.
    /// </summary>
    public interface IVendaService
    {
        Task<IEnumerable<Venda>> GetAllAsync(CancellationToken ct = default);
        Task<Venda?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Registra uma venda direta para um apartamento disponível, alterando seu status para Vendido.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o apartamento não existe ou não está no status Disponível.
        /// </exception>
        Task<Venda> CreateAsync(Venda venda, CancellationToken ct = default);

        Task UpdateAsync(Venda venda, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
