using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Application.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Services
{
    public class ReservaServiceTests
    {
        private static (AppDbContext context, UnitOfWork uow, ReservaService service) BuildSut(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var context = new AppDbContext(options);
            var uow = new UnitOfWork(context);
            var service = new ReservaService(uow, NullLogger<ReservaService>.Instance);
            return (context, uow, service);
        }

        [Fact]
        public async Task Criar_DeveMudarApartamentoParaReservado_QuandoDisponivel()
        {
            var (context, uow, service) = BuildSut("TestDb_CreateReserva");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                ClienteId = cliente.Id,
                ApartamentoId = apt.Id
            };

            var result = await service.CreateAsync(reserva);

            Assert.True(result.IsSuccess);
            var updatedApt = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Reservado, updatedApt!.Status);
            Assert.Equal(reserva.Id, result.Value.Id);

            context.Dispose();
        }

        [Fact]
        public async Task Criar_DeveRetornarFalha_QuandoClienteNaoEncontrado()
        {
            var (context, _, service) = BuildSut("TestDb_CreateReserva_ClienteNotFound");

            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                ClienteId = Guid.NewGuid(),
                ApartamentoId = apt.Id
            };

            var result = await service.CreateAsync(reserva);

            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
            context.Dispose();
        }

        [Theory]
        [InlineData(DesafioTecnico.Domain.Enums.StatusApartamento.Reservado)]
        [InlineData(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido)]
        public async Task Criar_DeveRetornarFalha_QuandoApartamentoNaoDisponivel(DesafioTecnico.Domain.Enums.StatusApartamento status)
        {
            var (context, _, service) = BuildSut($"TestDb_CreateReserva_NotAvailable_{status}");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            apt.Status = status;
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                ClienteId = cliente.Id,
                ApartamentoId = apt.Id
            };

            var result = await service.CreateAsync(reserva);

            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
            context.Dispose();
        }

        [Fact]
        public async Task Confirmar_DeveRetornarFalha_QuandoReservaNaoEncontrada()
        {
            var (context, _, service) = BuildSut("TestDb_ConfirmReserva_NotFound");

            var result = await service.ConfirmAsync(Guid.NewGuid());

            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
            context.Dispose();
        }

        [Theory]
        [InlineData(DesafioTecnico.Domain.Enums.StatusReserva.Confirmada)]
        [InlineData(DesafioTecnico.Domain.Enums.StatusReserva.Cancelada)]
        public async Task Confirmar_DeveRetornarFalha_QuandoReservaNaoPendente(DesafioTecnico.Domain.Enums.StatusReserva status)
        {
            var (context, _, service) = BuildSut($"TestDb_ConfirmReserva_NotPending_{status}");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);

            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                Id = Guid.NewGuid(),
                ClienteId = cliente.Id,
                ApartamentoId = apt.Id,
                DataReserva = DateTime.UtcNow,
                Status = status
            };
            context.Reservas.Add(reserva);
            context.SaveChanges();

            var result = await service.ConfirmAsync(reserva.Id);

            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
            context.Dispose();
        }

        [Theory]
        [InlineData(DesafioTecnico.Domain.Enums.StatusReserva.Confirmada)]
        [InlineData(DesafioTecnico.Domain.Enums.StatusReserva.Cancelada)]
        public async Task Cancelar_DeveRetornarFalha_QuandoReservaNaoPendente(DesafioTecnico.Domain.Enums.StatusReserva status)
        {
            var (context, _, service) = BuildSut($"TestDb_CancelReserva_NotPending_{status}");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);

            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                Id = Guid.NewGuid(),
                ClienteId = cliente.Id,
                ApartamentoId = apt.Id,
                DataReserva = DateTime.UtcNow,
                Status = status
            };
            context.Reservas.Add(reserva);
            context.SaveChanges();

            var result = await service.CancelAsync(reserva.Id);

            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
            context.Dispose();
        }

        [Fact]
        public async Task Confirmar_DeveRetornarFalha_QuandoApartamentoDaReservaNaoEncontrado()
        {
            var (context, _, service) = BuildSut("TestDb_ConfirmReserva_AptNotFound");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            context.Clientes.Add(cliente);

            // Reserva aponta para um ApartamentoId que não existe no banco
            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                Id = Guid.NewGuid(),
                ClienteId = cliente.Id,
                ApartamentoId = Guid.NewGuid(),
                DataReserva = DateTime.UtcNow,
                Status = DesafioTecnico.Domain.Enums.StatusReserva.Pendente
            };
            context.Reservas.Add(reserva);
            context.SaveChanges();

            var result = await service.ConfirmAsync(reserva.Id);

            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
            context.Dispose();
        }

        [Fact]
        public async Task Confirmar_DeveCriarVenda_EMudarApartamentoParaVendido()
        {
            var (context, uow, service) = BuildSut("TestDb_ConfirmReserva");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                ClienteId = cliente.Id,
                ApartamentoId = apt.Id
            };

            var created = await service.CreateAsync(reserva);
            Assert.True(created.IsSuccess);

            var confirm = await service.ConfirmAsync(created.Value.Id);
            Assert.True(confirm.IsSuccess);

            var updatedApt = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido, updatedApt!.Status);

            var vendas = await uow.Vendas.GetAllAsync();
            var venda = vendas.FirstOrDefault(v => v.ApartamentoId == apt.Id && v.ClienteId == cliente.Id);
            Assert.NotNull(venda);

            var updatedReserva = await uow.Reservas.GetByIdAsync(created.Value.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusReserva.Confirmada, updatedReserva!.Status);

            context.Dispose();
        }

        [Fact]
        public async Task Deletar_DeveLiberarApartamento_QuandoReservaPendente()
        {
            var (context, uow, service) = BuildSut("TestDb_DeleteReserva_PendenteLibera");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var reserva = new DesafioTecnico.Domain.Entities.Reserva { ClienteId = cliente.Id, ApartamentoId = apt.Id };
            var created = await service.CreateAsync(reserva);
            Assert.True(created.IsSuccess);

            await service.DeleteAsync(created.Value.Id);

            var aptApos = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Disponivel, aptApos!.Status);

            var reservaApos = await uow.Reservas.GetByIdAsync(created.Value.Id);
            Assert.Null(reservaApos);

            context.Dispose();
        }

        [Fact]
        public async Task Deletar_DeveRetornarFalha_QuandoReservaConfirmada()
        {
            var (context, uow, service) = BuildSut("TestDb_DeleteReserva_ConfirmadaBloqueada");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var reserva = new DesafioTecnico.Domain.Entities.Reserva { ClienteId = cliente.Id, ApartamentoId = apt.Id };
            var created = await service.CreateAsync(reserva);
            Assert.True(created.IsSuccess);
            await service.ConfirmAsync(created.Value.Id);

            var result = await service.DeleteAsync(created.Value.Id);

            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
            var reservaApos = await uow.Reservas.GetByIdAsync(created.Value.Id);
            Assert.NotNull(reservaApos);

            context.Dispose();
        }

        [Fact]
        public async Task Cancelar_DeveDevolverApartamentoParaDisponivel_EMudarStatusParaCancelada()
        {
            var (context, uow, service) = BuildSut("TestDb_CancelReserva");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                ClienteId = cliente.Id,
                ApartamentoId = apt.Id
            };

            var created = await service.CreateAsync(reserva);
            Assert.True(created.IsSuccess);

            var cancel = await service.CancelAsync(created.Value.Id);
            Assert.True(cancel.IsSuccess);

            var updatedApt = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Disponivel, updatedApt!.Status);

            var updatedReserva = await uow.Reservas.GetByIdAsync(created.Value.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusReserva.Cancelada, updatedReserva!.Status);

            context.Dispose();
        }
    }
}
