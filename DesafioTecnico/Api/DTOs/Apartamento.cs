using System.ComponentModel.DataAnnotations;

namespace DesafioTecnico.Api.DTOs
{
    public class ApartamentoReadDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Bloco { get; set; } = string.Empty;
        public int Andar { get; set; }
        public decimal Area { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ApartamentoCreateDto
    {
        [Required]
        public string Codigo { get; set; } = string.Empty;
        public string Bloco { get; set; } = string.Empty;
        [Required]
        public int Andar { get; set; }
        [Required]
        public decimal Area { get; set; }
        [Required]
        public decimal Valor { get; set; }
    }

    public class ApartamentoUpdateDto : ApartamentoCreateDto
    {
    }
}
