using System.ComponentModel.DataAnnotations;

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
        public Guid ClienteId { get; set; }
        [Required]
        public Guid ApartamentoId { get; set; }
        [Required]
        public decimal ValorPago { get; set; }
    }

    public class VendaUpdateDto : VendaCreateDto
    {
    }
}
