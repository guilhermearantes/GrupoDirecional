using System.ComponentModel.DataAnnotations;

namespace DesafioTecnico.Api.DTOs
{
    public class ReservaCreateDto
    {
        [Required]
        public Guid ClienteId { get; set; }
        [Required]
        public Guid ApartamentoId { get; set; }
    }

    public class ReservaReadDto
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public Guid ApartamentoId { get; set; }
        public DateTime DataReserva { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
