using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Results;

namespace DesafioTecnico.Application.Services.Interfaces
{
    /// <summary>
    /// Define operações relacionadas ao gerenciamento de vendas diretas de apartamentos.
    /// </summary>
    public interface IVendaService
    {
        Task<IEnumerable<Venda>> GetAllAsync(CancellationToken ct = default);
        Task<Venda?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<(IEnumerable<Venda> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

        /// <summary>
        /// Registra uma venda direta para um apartamento disponível, alterando seu status para Vendido.
        /// Retorna <see cref="Result{T}"/> com falha se o apartamento não existir ou não estiver Disponível.
        /// </summary>
        Task<Result<Venda>> CreateAsync(Venda venda, CancellationToken ct = default);

        Task UpdateAsync(Venda venda, CancellationToken ct = default);
        Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
