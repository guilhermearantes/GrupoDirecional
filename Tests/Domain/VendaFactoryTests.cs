using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Factories;

namespace Tests.Domain
{
    public class VendaFactoryTests
    {
        [Fact]
        public void CriarVendaDireta_DevePopularCamposObrigatorios()
        {
            var clienteId = Guid.NewGuid();
            var apartamentoId = Guid.NewGuid();
            const decimal valorPago = 350_000m;

            var venda = VendaFactory.CriarVendaDireta(clienteId, apartamentoId, valorPago);

            Assert.NotEqual(Guid.Empty, venda.Id);
            Assert.Equal(clienteId, venda.ClienteId);
            Assert.Equal(apartamentoId, venda.ApartamentoId);
            Assert.Equal(valorPago, venda.ValorPago);
            Assert.True(venda.DataVenda > DateTime.UtcNow.AddSeconds(-5));
        }

        [Fact]
        public void CriarPorReserva_DeveUsarDadosDaReservaEValorDoApartamento()
        {
            var reserva = new Reserva
            {
                Id = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                ApartamentoId = Guid.NewGuid()
            };
            var apt = new Apartamento { Valor = 420_000m };

            var venda = VendaFactory.CriarPorReserva(reserva, apt);

            Assert.NotEqual(Guid.Empty, venda.Id);
            Assert.Equal(reserva.ClienteId, venda.ClienteId);
            Assert.Equal(reserva.ApartamentoId, venda.ApartamentoId);
            Assert.Equal(apt.Valor, venda.ValorPago);
            Assert.True(venda.DataVenda > DateTime.UtcNow.AddSeconds(-5));
        }

        [Fact]
        public void CriarVendaDireta_DeveGerarIdUnico_CadaVez()
        {
            var clienteId = Guid.NewGuid();
            var apartamentoId = Guid.NewGuid();

            var v1 = VendaFactory.CriarVendaDireta(clienteId, apartamentoId, 100m);
            var v2 = VendaFactory.CriarVendaDireta(clienteId, apartamentoId, 100m);

            Assert.NotEqual(v1.Id, v2.Id);
        }
    }
}
