using System;
using System.Collections.Generic;

namespace DesafioTecnico.Domain.Entities
{
    public class Cliente
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    }
}
