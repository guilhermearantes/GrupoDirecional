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
            var result = apt.Reservar();
            Assert.True(result.IsSuccess);
            Assert.Equal(StatusApartamento.Reservado, apt.Status);
        }

        [Theory]
        [InlineData(StatusApartamento.Reservado)]
        [InlineData(StatusApartamento.Vendido)]
        public void Reservar_DeveRetornarFalha_QuandoNaoDisponivel(StatusApartamento status)
        {
            var apt = new Apartamento { Status = status };
            var result = apt.Reservar();
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
        }

        // ── Apartamento.Vender (via confirmação de reserva) ──────────────────

        [Fact]
        public void Vender_DeveMudarStatusParaVendido_QuandoReservado()
        {
            var apt = new Apartamento { Status = StatusApartamento.Reservado };
            var result = apt.Vender();
            Assert.True(result.IsSuccess);
            Assert.Equal(StatusApartamento.Vendido, apt.Status);
        }

        [Theory]
        [InlineData(StatusApartamento.Disponivel)]
        [InlineData(StatusApartamento.Vendido)]
        public void Vender_DeveRetornarFalha_QuandoNaoReservado(StatusApartamento status)
        {
            var apt = new Apartamento { Status = status };
            var result = apt.Vender();
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
        }

        // ── Apartamento.VenderDiretamente (POST /vendas sem reserva) ─────────

        [Fact]
        public void VenderDiretamente_DeveMudarStatusParaVendido_QuandoDisponivel()
        {
            var apt = new Apartamento { Status = StatusApartamento.Disponivel };
            var result = apt.VenderDiretamente();
            Assert.True(result.IsSuccess);
            Assert.Equal(StatusApartamento.Vendido, apt.Status);
        }

        [Theory]
        [InlineData(StatusApartamento.Reservado)]
        [InlineData(StatusApartamento.Vendido)]
        public void VenderDiretamente_DeveRetornarFalha_QuandoNaoDisponivel(StatusApartamento status)
        {
            var apt = new Apartamento { Status = status };
            var result = apt.VenderDiretamente();
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
        }

        // ── Apartamento.Liberar ───────────────────────────────────────────────

        [Fact]
        public void Liberar_DeveMudarStatusParaDisponivel_QuandoReservado()
        {
            var apt = new Apartamento { Status = StatusApartamento.Reservado };
            var result = apt.Liberar();
            Assert.True(result.IsSuccess);
            Assert.Equal(StatusApartamento.Disponivel, apt.Status);
        }

        [Theory]
        [InlineData(StatusApartamento.Disponivel)]
        [InlineData(StatusApartamento.Vendido)]
        public void Liberar_DeveRetornarFalha_QuandoNaoReservado(StatusApartamento status)
        {
            var apt = new Apartamento { Status = status };
            var result = apt.Liberar();
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
        }

        // ── Apartamento.EstornarVenda ─────────────────────────────────────────

        [Fact]
        public void EstornarVenda_DeveMudarStatusParaDisponivel_QuandoVendido()
        {
            var apt = new Apartamento { Status = StatusApartamento.Vendido };
            var result = apt.EstornarVenda();
            Assert.True(result.IsSuccess);
            Assert.Equal(StatusApartamento.Disponivel, apt.Status);
        }

        [Theory]
        [InlineData(StatusApartamento.Disponivel)]
        [InlineData(StatusApartamento.Reservado)]
        public void EstornarVenda_DeveRetornarFalha_QuandoNaoVendido(StatusApartamento status)
        {
            var apt = new Apartamento { Status = status };
            var result = apt.EstornarVenda();
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
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
            var result = reserva.Confirmar();
            Assert.True(result.IsSuccess);
            Assert.Equal(StatusReserva.Confirmada, reserva.Status);
        }

        [Theory]
        [InlineData(StatusReserva.Confirmada)]
        [InlineData(StatusReserva.Cancelada)]
        public void Confirmar_DeveRetornarFalha_QuandoNaoPendente(StatusReserva status)
        {
            var reserva = new Reserva { Status = status };
            var result = reserva.Confirmar();
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
        }

        // ── Reserva.Cancelar ──────────────────────────────────────────────────

        [Fact]
        public void Cancelar_DeveMudarStatusParaCancelada_QuandoPendente()
        {
            var reserva = new Reserva { Status = StatusReserva.Pendente };
            var result = reserva.Cancelar();
            Assert.True(result.IsSuccess);
            Assert.Equal(StatusReserva.Cancelada, reserva.Status);
        }

        [Theory]
        [InlineData(StatusReserva.Confirmada)]
        [InlineData(StatusReserva.Cancelada)]
        public void Cancelar_DeveRetornarFalha_QuandoNaoPendente(StatusReserva status)
        {
            var reserva = new Reserva { Status = status };
            var result = reserva.Cancelar();
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
        }
    }
}
