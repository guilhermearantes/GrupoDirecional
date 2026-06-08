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

        /// <summary>Tenta alterar o status para Reservado. Retorna falha se o apartamento não estiver Disponível.</summary>
        public Result Reservar()
        {
            if (Status != Enums.StatusApartamento.Disponivel)
                return Result.Fail("Apartamento não está disponível para reserva.");
            Status = Enums.StatusApartamento.Reservado;
            return Result.Ok();
        }

        /// <summary>Tenta alterar o status para Vendido a partir de uma reserva confirmada. Retorna falha se não estiver Reservado.</summary>
        public Result Vender()
        {
            if (Status != Enums.StatusApartamento.Reservado)
                return Result.Fail("Somente apartamentos reservados podem ser vendidos.");
            Status = Enums.StatusApartamento.Vendido;
            return Result.Ok();
        }

        /// <summary>Tenta alterar o status para Vendido sem reserva prévia. Retorna falha se não estiver Disponível.</summary>
        public Result VenderDiretamente()
        {
            if (Status != Enums.StatusApartamento.Disponivel)
                return Result.Fail("Venda direta só é permitida em apartamentos disponíveis.");
            Status = Enums.StatusApartamento.Vendido;
            return Result.Ok();
        }

        /// <summary>Tenta devolver o apartamento ao status Disponível. Retorna falha se não estiver Reservado.</summary>
        public Result Liberar()
        {
            if (Status != Enums.StatusApartamento.Reservado)
                return Result.Fail("Somente apartamentos reservados podem ser liberados.");
            Status = Enums.StatusApartamento.Disponivel;
            return Result.Ok();
        }
    }
}
