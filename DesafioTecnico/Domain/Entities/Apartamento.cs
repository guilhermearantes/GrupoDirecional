namespace DesafioTecnico.Domain.Entities
{
    public class Apartamento
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Bloco { get; set; } = string.Empty;
        public int Andar { get; set; }
        public decimal Area { get; set; }
        public decimal Valor { get; set; }
        public Enums.StatusApartamento Status { get; set; }

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Venda> Vendas { get; set; } = new List<Venda>();

        /// <summary>
        /// Altera o status para Reservado.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o apartamento não está Disponível.
        /// </exception>
        public void Reservar()
        {
            if (Status != Enums.StatusApartamento.Disponivel)
                throw new InvalidOperationException("Apartamento não está disponível para reserva.");
            Status = Enums.StatusApartamento.Reservado;
        }

        /// <summary>
        /// Altera o status para Vendido a partir de uma reserva confirmada.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o apartamento não está Reservado.
        /// </exception>
        public void Vender()
        {
            if (Status != Enums.StatusApartamento.Reservado)
                throw new InvalidOperationException("Somente apartamentos reservados podem ser vendidos.");
            Status = Enums.StatusApartamento.Vendido;
        }

        /// <summary>
        /// Altera o status para Vendido sem reserva prévia (venda direta).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o apartamento não está Disponível.
        /// </exception>
        public void VenderDiretamente()
        {
            if (Status != Enums.StatusApartamento.Disponivel)
                throw new InvalidOperationException("Venda direta só é permitida em apartamentos disponíveis.");
            Status = Enums.StatusApartamento.Vendido;
        }

        /// <summary>
        /// Devolve o apartamento ao status Disponível após cancelamento de reserva.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o apartamento não está Reservado.
        /// </exception>
        public void Liberar()
        {
            if (Status != Enums.StatusApartamento.Reservado)
                throw new InvalidOperationException("Somente apartamentos reservados podem ser liberados.");
            Status = Enums.StatusApartamento.Disponivel;
        }
    }
}
