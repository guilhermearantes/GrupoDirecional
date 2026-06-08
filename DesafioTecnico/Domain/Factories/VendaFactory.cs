using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Domain.Factories
{
    /// <summary>
    /// Centraliza a criação de instâncias de <see cref="Venda"/>, garantindo
    /// que <c>Id</c> e <c>DataVenda</c> sejam sempre atribuídos no momento da criação.
    /// </summary>
    public static class VendaFactory
    {
        /// <summary>
        /// Cria uma venda originada diretamente pelo comprador, sem reserva prévia.
        /// </summary>
        public static Venda CriarVendaDireta(Guid clienteId, Guid apartamentoId, decimal valorPago)
            => new()
            {
                Id = Guid.NewGuid(),
                ClienteId = clienteId,
                ApartamentoId = apartamentoId,
                ValorPago = valorPago,
                DataVenda = DateTime.UtcNow
            };

        /// <summary>
        /// Cria a venda gerada automaticamente ao confirmar uma reserva.
        /// O valor pago assume o valor atual do apartamento no momento da confirmação.
        /// </summary>
        public static Venda CriarPorReserva(Reserva reserva, Apartamento apartamento)
            => new()
            {
                Id = Guid.NewGuid(),
                ClienteId = reserva.ClienteId,
                ApartamentoId = reserva.ApartamentoId,
                ValorPago = apartamento.Valor,
                DataVenda = DateTime.UtcNow
            };
    }
}
