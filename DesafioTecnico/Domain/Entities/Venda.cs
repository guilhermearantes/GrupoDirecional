namespace DesafioTecnico.Domain.Entities
{
    public class Venda
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public Guid ApartamentoId { get; set; }
        public DateTime DataVenda { get; set; }
        public decimal ValorPago { get; set; }

        public Cliente? Cliente { get; set; }
        public Apartamento? Apartamento { get; set; }
    }
}
