using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Define operações relacionadas ao ciclo de vida de reservas de apartamentos.
    /// </summary>
    public interface IReservaService
    {
        Task<IEnumerable<Reserva>> GetAllAsync(CancellationToken ct = default);
        Task<Reserva?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Cria uma nova reserva para um apartamento disponível, alterando seu status para Reservado.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o apartamento não existe ou não está disponível.
        /// </exception>
        Task<Reserva> CreateAsync(Reserva reserva, CancellationToken ct = default);

        /// <summary>
        /// Confirma uma reserva pendente, gera a venda correspondente e marca o apartamento como Vendido.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a reserva não existe ou não está no estado Pendente.
        /// </exception>
        Task ConfirmAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Cancela uma reserva pendente e devolve o apartamento ao status Disponível.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a reserva não existe ou não está no estado Pendente.
        /// </exception>
        Task CancelAsync(Guid id, CancellationToken ct = default);

        Task UpdateAsync(Reserva reserva, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
