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
    }
}
