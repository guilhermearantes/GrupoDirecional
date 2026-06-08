using DesafioTecnico.Domain.Results;

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

        public Result Reservar()
        {
            if (Status != Enums.StatusApartamento.Disponivel)
                return Result.Fail("Apartamento não está disponível para reserva.");
            Status = Enums.StatusApartamento.Reservado;
            return Result.Ok();
        }

        public Result Vender()
        {
            if (Status != Enums.StatusApartamento.Reservado)
                return Result.Fail("Somente apartamentos reservados podem ser vendidos.");
            Status = Enums.StatusApartamento.Vendido;
            return Result.Ok();
        }

        public Result VenderDiretamente()
        {
            if (Status != Enums.StatusApartamento.Disponivel)
                return Result.Fail("Venda direta só é permitida em apartamentos disponíveis.");
            Status = Enums.StatusApartamento.Vendido;
            return Result.Ok();
        }

        public Result Liberar()
        {
            if (Status != Enums.StatusApartamento.Reservado)
                return Result.Fail("Somente apartamentos reservados podem ser liberados.");
            Status = Enums.StatusApartamento.Disponivel;
            return Result.Ok();
        }

        public Result EstornarVenda()
        {
            if (Status != Enums.StatusApartamento.Vendido)
                return Result.Fail("Somente apartamentos vendidos podem ser estornados.");
            Status = Enums.StatusApartamento.Disponivel;
            return Result.Ok();
        }
    }
}
