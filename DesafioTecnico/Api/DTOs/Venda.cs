using System.ComponentModel.DataAnnotations;
using DesafioTecnico.Api.Validation;

namespace DesafioTecnico.Api.DTOs
{
    public class VendaReadDto
    {
        /// <summary>Identificador único da venda.</summary>
        public Guid Id { get; set; }

        /// <summary>Identificador do cliente comprador.</summary>
        public Guid ClienteId { get; set; }

        /// <summary>Identificador do apartamento vendido.</summary>
        public Guid ApartamentoId { get; set; }

        /// <summary>Data e hora da venda (UTC).</summary>
        public DateTime DataVenda { get; set; }

        /// <summary>Valor efetivamente pago na transação em reais.</summary>
        /// <example>420000.00</example>
        public decimal ValorPago { get; set; }
    }

    public class VendaCreateDto
    {
        /// <summary>Identificador do cliente comprador.</summary>
        [Required]
        [NotEmptyGuid]
        public Guid ClienteId { get; set; }

        /// <summary>Identificador do apartamento. Deve estar com status <c>Disponivel</c> para venda direta.</summary>
        [Required]
        [NotEmptyGuid]
        public Guid ApartamentoId { get; set; }

        /// <summary>Valor pago pela unidade em reais.</summary>
        /// <example>420000.00</example>
        [Required]
        public decimal ValorPago { get; set; }
    }

    public class VendaUpdateDto
    {
        /// <summary>Novo valor pago pela unidade em reais.</summary>
        /// <example>420000.00</example>
        [Required]
        public decimal ValorPago { get; set; }
    }
}
