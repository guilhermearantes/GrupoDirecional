using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Enums;

namespace Tests.Domain
{
    public class DomainEntityTests
    {
        // ── Apartamento.Reservar ─────────────────────────────────────────────

        [Fact]
        public void Reservar_DeveMudarStatusParaReservado_QuandoDisponivel()
        {
            var apt = new Apartamento { Status = StatusApartamento.Disponivel };
            apt.Reservar();
            Assert.Equal(StatusApartamento.Reservado, apt.Status);
        }

        [Theory]
        [InlineData(StatusApartamento.Reservado)]
        [InlineData(StatusApartamento.Vendido)]
        public void Reservar_DeveLancarExcecao_QuandoNaoDisponivel(StatusApartamento status)
        {
            var apt = new Apartamento { Status = status };
            Assert.Throws<InvalidOperationException>(() => apt.Reservar());
        }

        // ── Apartamento.Vender (via confirmação de reserva) ──────────────────

        [Fact]
        public void Vender_DeveMudarStatusParaVendido_QuandoReservado()
        {
            var apt = new Apartamento { Status = StatusApartamento.Reservado };
            apt.Vender();
            Assert.Equal(StatusApartamento.Vendido, apt.Status);
        }

        [Theory]
        [InlineData(StatusApartamento.Disponivel)]
        [InlineData(StatusApartamento.Vendido)]
        public void Vender_DeveLancarExcecao_QuandoNaoReservado(StatusApartamento status)
        {
            var apt = new Apartamento { Status = status };
            Assert.Throws<InvalidOperationException>(() => apt.Vender());
        }

        // ── Apartamento.VenderDiretamente (POST /vendas sem reserva) ─────────

        [Fact]
        public void VenderDiretamente_DeveMudarStatusParaVendido_QuandoDisponivel()
        {
            var apt = new Apartamento { Status = StatusApartamento.Disponivel };
            apt.VenderDiretamente();
            Assert.Equal(StatusApartamento.Vendido, apt.Status);
        }

        [Theory]
        [InlineData(StatusApartamento.Reservado)]
        [InlineData(StatusApartamento.Vendido)]
        public void VenderDiretamente_DeveLancarExcecao_QuandoNaoDisponivel(StatusApartamento status)
        {
            var apt = new Apartamento { Status = status };
            Assert.Throws<InvalidOperationException>(() => apt.VenderDiretamente());
        }

        // ── Apartamento.Liberar ───────────────────────────────────────────────

        [Fact]
        public void Liberar_DeveMudarStatusParaDisponivel_QuandoReservado()
        {
            var apt = new Apartamento { Status = StatusApartamento.Reservado };
            apt.Liberar();
            Assert.Equal(StatusApartamento.Disponivel, apt.Status);
        }

        [Theory]
        [InlineData(StatusApartamento.Disponivel)]
        [InlineData(StatusApartamento.Vendido)]
        public void Liberar_DeveLancarExcecao_QuandoNaoReservado(StatusApartamento status)
        {
            var apt = new Apartamento { Status = status };
            Assert.Throws<InvalidOperationException>(() => apt.Liberar());
        }

        // ── Reserva.Iniciar ───────────────────────────────────────────────────

        [Fact]
        public void Iniciar_DeveDefinirIdDataEStatusPendente_QuandoChamado()
        {
            var reserva = new Reserva();
            reserva.Iniciar();
            Assert.NotEqual(Guid.Empty, reserva.Id);
            Assert.Equal(StatusReserva.Pendente, reserva.Status);
            Assert.True(reserva.DataReserva > DateTime.UtcNow.AddSeconds(-5));
        }

        // ── Reserva.Confirmar ─────────────────────────────────────────────────

        [Fact]
        public void Confirmar_DeveMudarStatusParaConfirmada_QuandoPendente()
        {
            var reserva = new Reserva { Status = StatusReserva.Pendente };
            reserva.Confirmar();
            Assert.Equal(StatusReserva.Confirmada, reserva.Status);
        }

        [Theory]
        [InlineData(StatusReserva.Confirmada)]
        [InlineData(StatusReserva.Cancelada)]
        public void Confirmar_DeveLancarExcecao_QuandoNaoPendente(StatusReserva status)
        {
            var reserva = new Reserva { Status = status };
            Assert.Throws<InvalidOperationException>(() => reserva.Confirmar());
        }

        // ── Reserva.Cancelar ──────────────────────────────────────────────────

        [Fact]
        public void Cancelar_DeveMudarStatusParaCancelada_QuandoPendente()
        {
            var reserva = new Reserva { Status = StatusReserva.Pendente };
            reserva.Cancelar();
            Assert.Equal(StatusReserva.Cancelada, reserva.Status);
        }

        [Theory]
        [InlineData(StatusReserva.Confirmada)]
        [InlineData(StatusReserva.Cancelada)]
        public void Cancelar_DeveLancarExcecao_QuandoNaoPendente(StatusReserva status)
        {
            var reserva = new Reserva { Status = status };
            Assert.Throws<InvalidOperationException>(() => reserva.Cancelar());
        }
    }
}
