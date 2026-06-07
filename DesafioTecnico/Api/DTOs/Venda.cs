using System.ComponentModel.DataAnnotations;
using DesafioTecnico.Api.Validation;

namespace DesafioTecnico.Api.DTOs
{
    public class VendaReadDto
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public Guid ApartamentoId { get; set; }
        public DateTime DataVenda { get; set; }
        public decimal ValorPago { get; set; }
    }

    public class VendaCreateDto
    {
        [Required]
        [NotEmptyGuid]
        public Guid ClienteId { get; set; }

        [Required]
        [NotEmptyGuid]
        public Guid ApartamentoId { get; set; }

        [Required]
        public decimal ValorPago { get; set; }
    }

    public class VendaUpdateDto
    {
        [Required]
        public decimal ValorPago { get; set; }
    }
}
