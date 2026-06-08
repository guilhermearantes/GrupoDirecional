using DesafioTecnico.Domain.Results;

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

        /// <summary>Inicializa a reserva com novo Id, data atual e status Pendente.</summary>
        public void Iniciar()
        {
            Id = Guid.NewGuid();
            DataReserva = DateTime.UtcNow;
            Status = Enums.StatusReserva.Pendente;
        }

        /// <summary>Tenta confirmar a reserva. Retorna falha se não estiver no estado Pendente.</summary>
        public Result Confirmar()
        {
            if (Status != Enums.StatusReserva.Pendente)
                return Result.Fail("Reserva não está em estado pendente.");
            Status = Enums.StatusReserva.Confirmada;
            return Result.Ok();
        }

        /// <summary>Tenta cancelar a reserva. Retorna falha se não estiver no estado Pendente.</summary>
        public Result Cancelar()
        {
            if (Status != Enums.StatusReserva.Pendente)
                return Result.Fail("Somente reservas pendentes podem ser canceladas.");
            Status = Enums.StatusReserva.Cancelada;
            return Result.Ok();
        }
    }
}
