using System.ComponentModel.DataAnnotations;
using DesafioTecnico.Api.Validation;

namespace DesafioTecnico.Api.DTOs
{
    public class ClienteReadDto
    {
        /// <summary>Identificador único do cliente.</summary>
        public Guid Id { get; set; }

        /// <summary>Nome completo do cliente.</summary>
        /// <example>João da Silva</example>
        public string Nome { get; set; } = string.Empty;

        /// <summary>E-mail do cliente.</summary>
        /// <example>joao@exemplo.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>CPF no formato <c>NNN.NNN.NNN-NN</c>.</summary>
        /// <example>123.456.789-00</example>
        public string Cpf { get; set; } = string.Empty;

        /// <summary>Data de nascimento.</summary>
        /// <example>1985-06-15T00:00:00Z</example>
        public DateTime DataNascimento { get; set; }
    }

    public class ClienteCreateDto
    {
        /// <summary>Nome completo do cliente.</summary>
        /// <example>João da Silva</example>
        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        /// <summary>E-mail único do cliente.</summary>
        /// <example>joao@exemplo.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>CPF no formato <c>NNN.NNN.NNN-NN</c>.</summary>
        /// <example>123.456.789-00</example>
        [Required]
        [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "CPF deve estar no formato NNN.NNN.NNN-NN")]
        public string Cpf { get; set; } = string.Empty;

        /// <summary>Data de nascimento em UTC.</summary>
        /// <example>1985-06-15T00:00:00Z</example>
        [Required]
        [DataNascimentoValida]
        public DateTime DataNascimento { get; set; }
    }

    public class ClienteUpdateDto : ClienteCreateDto
    {
    }
}
