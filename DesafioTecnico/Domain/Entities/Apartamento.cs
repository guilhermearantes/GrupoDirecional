using System;

namespace DesafioTecnico.Domain.Entities
{
    public class Apartamento
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Bloco { get; set; } = string.Empty;
        public int Andar { get; set; }
        public decimal Area { get; set; }
        public decimal Valor { get; set; }
        public Domain.Enums.StatusApartamento Status { get; set; }

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    }
}
