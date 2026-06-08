using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Results;

namespace DesafioTecnico.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Define operações relacionadas ao ciclo de vida de reservas de apartamentos.
    /// </summary>
    public interface IReservaService
    {
        Task<IEnumerable<Reserva>> GetAllAsync(CancellationToken ct = default);
        Task<Reserva?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<(IEnumerable<Reserva> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

        /// <summary>
        /// Cria uma nova reserva para um apartamento disponível, alterando seu status para Reservado.
        /// Retorna <see cref="Result{T}"/> com falha se o apartamento não existir ou não estiver disponível.
        /// </summary>
        Task<Result<Reserva>> CreateAsync(Reserva reserva, CancellationToken ct = default);

        /// <summary>
        /// Confirma uma reserva pendente, gera a venda correspondente e marca o apartamento como Vendido.
        /// Retorna <see cref="Result"/> com falha se a reserva não existir ou não estiver no estado Pendente.
        /// </summary>
        Task<Result> ConfirmAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Cancela uma reserva pendente e devolve o apartamento ao status Disponível.
        /// Retorna <see cref="Result"/> com falha se a reserva não existir ou não estiver no estado Pendente.
        /// </summary>
        Task<Result> CancelAsync(Guid id, CancellationToken ct = default);

        Task UpdateAsync(Reserva reserva, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
