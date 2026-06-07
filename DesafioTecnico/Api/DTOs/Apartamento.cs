using System.ComponentModel.DataAnnotations;

namespace DesafioTecnico.Api.DTOs
{
    public class ApartamentoReadDto
    {
        /// <summary>Identificador único do apartamento.</summary>
        public Guid Id { get; set; }

        /// <summary>Código identificador do apartamento.</summary>
        /// <example>AP-101</example>
        public string Codigo { get; set; } = string.Empty;

        /// <summary>Bloco do condomínio.</summary>
        /// <example>A</example>
        public string Bloco { get; set; } = string.Empty;

        /// <summary>Número do andar.</summary>
        /// <example>10</example>
        public int Andar { get; set; }

        /// <summary>Área em metros quadrados.</summary>
        /// <example>75.50</example>
        public decimal Area { get; set; }

        /// <summary>Valor de venda em reais.</summary>
        /// <example>450000.00</example>
        public decimal Valor { get; set; }

        /// <summary>Status atual: <c>Disponivel</c>, <c>Reservado</c> ou <c>Vendido</c>.</summary>
        /// <example>Disponivel</example>
        public string Status { get; set; } = string.Empty;
    }

    public class ApartamentoCreateDto
    {
        /// <summary>Código único que identifica o apartamento (ex: AP-101).</summary>
        /// <example>AP-101</example>
        [Required]
        public string Codigo { get; set; } = string.Empty;

        /// <summary>Bloco do condomínio.</summary>
        /// <example>A</example>
        public string Bloco { get; set; } = string.Empty;

        /// <summary>Número do andar.</summary>
        /// <example>10</example>
        [Required]
        public int Andar { get; set; }

        /// <summary>Área em metros quadrados.</summary>
        /// <example>75.50</example>
        [Required]
        public decimal Area { get; set; }

        /// <summary>Valor de venda em reais.</summary>
        /// <example>450000.00</example>
        [Required]
        public decimal Valor { get; set; }
    }

    public class ApartamentoUpdateDto : ApartamentoCreateDto
    {
    }
}
