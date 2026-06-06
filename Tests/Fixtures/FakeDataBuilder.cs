using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Fixtures
{
    internal class FakeDataBuilder
    {
        public static DesafioTecnico.Domain.Entities.Cliente CreateCliente()
        {
            return new DesafioTecnico.Domain.Entities.Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente Teste",
                Email = "teste@exemplo.com",
                Cpf = "000.000.000-00",
                DataNascimento = DateTime.UtcNow.AddYears(-30)
            };
        }

        public static DesafioTecnico.Domain.Entities.Apartamento CreateApartamento()
        {
            return new DesafioTecnico.Domain.Entities.Apartamento
            {
                Id = Guid.NewGuid(),
                Codigo = $"TEST-{Guid.NewGuid():N}"[..12],
                Bloco = "A",
                Andar = 1,
                Area = 75.5m,
                Valor = 350000m,
                Status = DesafioTecnico.Domain.Enums.StatusApartamento.Disponivel
            };
        }

        public static DesafioTecnico.Domain.Entities.Venda CreateVenda(Guid clienteId, Guid aptId)
        {
            return new DesafioTecnico.Domain.Entities.Venda
            {
                Id = Guid.NewGuid(),
                ClienteId = clienteId,
                ApartamentoId = aptId,
                DataVenda = DateTime.UtcNow,
                ValorPago = 100000m
            };
        }
    }
}
