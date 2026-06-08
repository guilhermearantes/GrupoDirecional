using DesafioTecnico.Infrastructure.Repositories.Interfaces;

namespace DesafioTecnico.Infrastructure.Data
{
    /// <summary>
    /// Coordena o ciclo de vida de múltiplos repositórios e garante que todas as
    /// mudanças rastreadas sejam persistidas atomicamente via <see cref="CommitAsync"/>.
    /// </summary>
    public interface IUnitOfWork
    {
        IClienteRepository Clientes { get; }
        IApartamentoRepository Apartamentos { get; }
        IReservaRepository Reservas { get; }
        IVendaRepository Vendas { get; }

        /// <summary>
        /// Persiste todas as mudanças rastreadas no banco em uma única transação.
        /// </summary>
        Task<int> CommitAsync(CancellationToken ct = default);
    }
}
