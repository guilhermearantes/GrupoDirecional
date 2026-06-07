using System.ComponentModel.DataAnnotations;
using DesafioTecnico.Api.Validation;

namespace DesafioTecnico.Api.DTOs
{
    public class ReservaCreateDto
    {
        [Required]
        [NotEmptyGuid]
        public Guid ClienteId { get; set; }

        [Required]
        [NotEmptyGuid]
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
