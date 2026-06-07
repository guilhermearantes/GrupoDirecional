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
        public Enums.StatusApartamento Status { get; set; }

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Venda> Vendas { get; set; } = new List<Venda>();

        public void Reservar()
        {
            if (Status != Enums.StatusApartamento.Disponivel)
                throw new InvalidOperationException("Apartamento não está disponível para reserva.");
            Status = Enums.StatusApartamento.Reservado;
        }

        public void Vender()
        {
            if (Status != Enums.StatusApartamento.Reservado)
                throw new InvalidOperationException("Somente apartamentos reservados podem ser vendidos.");
            Status = Enums.StatusApartamento.Vendido;
        }

        public void Liberar()
        {
            if (Status != Enums.StatusApartamento.Reservado)
                throw new InvalidOperationException("Somente apartamentos reservados podem ser liberados.");
            Status = Enums.StatusApartamento.Disponivel;
        }
    }
}
