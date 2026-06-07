using System.ComponentModel.DataAnnotations;
using DesafioTecnico.Api.Validation;

namespace DesafioTecnico.Api.DTOs
{
    public class ReservaCreateDto
    {
        /// <summary>Identificador do cliente que está realizando a reserva.</summary>
        [Required]
        [NotEmptyGuid]
        public Guid ClienteId { get; set; }

        /// <summary>Identificador do apartamento a ser reservado. Deve estar com status <c>Disponivel</c>.</summary>
        [Required]
        [NotEmptyGuid]
        public Guid ApartamentoId { get; set; }
    }

    public class ReservaReadDto
    {
        /// <summary>Identificador único da reserva.</summary>
        public Guid Id { get; set; }

        /// <summary>Identificador do cliente.</summary>
        public Guid ClienteId { get; set; }

        /// <summary>Identificador do apartamento reservado.</summary>
        public Guid ApartamentoId { get; set; }

        /// <summary>Data e hora da criação da reserva (UTC).</summary>
        public DateTime DataReserva { get; set; }

        /// <summary>Status atual da reserva: <c>Pendente</c>, <c>Confirmada</c> ou <c>Cancelada</c>.</summary>
        /// <example>Pendente</example>
        public string Status { get; set; } = string.Empty;
    }
}
