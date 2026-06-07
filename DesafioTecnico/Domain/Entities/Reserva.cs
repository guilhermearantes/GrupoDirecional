namespace DesafioTecnico.Domain.Entities
{
    public class Reserva
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public Guid ApartamentoId { get; set; }
        public DateTime DataReserva { get; set; }
        public Enums.StatusReserva Status { get; set; }

        public Cliente? Cliente { get; set; }
        public Apartamento? Apartamento { get; set; }

        /// <summary>
        /// Inicializa a reserva com novo Id, data atual e status Pendente.
        /// </summary>
        public void Iniciar()
        {
            Id = Guid.NewGuid();
            DataReserva = DateTime.UtcNow;
            Status = Enums.StatusReserva.Pendente;
        }

        /// <summary>
        /// Confirma a reserva, avançando o status de Pendente para Confirmada.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a reserva não está no estado Pendente.
        /// </exception>
        public void Confirmar()
        {
            if (Status != Enums.StatusReserva.Pendente)
                throw new InvalidOperationException("Reserva não está em estado pendente.");
            Status = Enums.StatusReserva.Confirmada;
        }

        /// <summary>
        /// Cancela a reserva, alterando o status de Pendente para Cancelada.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a reserva não está no estado Pendente.
        /// </exception>
        public void Cancelar()
        {
            if (Status != Enums.StatusReserva.Pendente)
                throw new InvalidOperationException("Somente reservas pendentes podem ser canceladas.");
            Status = Enums.StatusReserva.Cancelada;
        }
    }
}
