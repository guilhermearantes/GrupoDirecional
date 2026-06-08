using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Services;
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

            var created = await service.CreateAsync(reserva);

            var updatedApt = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Reservado, updatedApt!.Status);
            Assert.Equal(reserva.Id, created.Id);

            context.Dispose();
        }

        [Fact]
        public async Task Criar_DeveLancarExcecao_QuandoApartamentoNaoDisponivel()
        {
            var (context, _, service) = BuildSut("TestDb_CreateReserva_NotAvailable");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            apt.Status = DesafioTecnico.Domain.Enums.StatusApartamento.Vendido;
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                ClienteId = cliente.Id,
                ApartamentoId = apt.Id
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.CreateAsync(reserva));
            context.Dispose();
        }

        [Fact]
        public async Task Confirmar_DeveLancarExcecao_QuandoReservaNaoEncontrada()
        {
            var (context, _, service) = BuildSut("TestDb_ConfirmReserva_NotFound");

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.ConfirmAsync(Guid.NewGuid()));
            context.Dispose();
        }

        [Fact]
        public async Task Confirmar_DeveLancarExcecao_QuandoReservaJaConfirmada()
        {
            var (context, _, service) = BuildSut("TestDb_ConfirmReserva_NotPending");

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
                Status = DesafioTecnico.Domain.Enums.StatusReserva.Confirmada
            };
            context.Reservas.Add(reserva);
            context.SaveChanges();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.ConfirmAsync(reserva.Id));
            context.Dispose();
        }

        [Fact]
        public async Task Cancelar_DeveLancarExcecao_QuandoReservaJaConfirmada()
        {
            var (context, _, service) = BuildSut("TestDb_CancelReserva_NotPending");

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
                Status = DesafioTecnico.Domain.Enums.StatusReserva.Confirmada
            };
            context.Reservas.Add(reserva);
            context.SaveChanges();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.CancelAsync(reserva.Id));
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
            await service.ConfirmAsync(created.Id);

            var updatedApt = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido, updatedApt!.Status);

            var vendas = await uow.Vendas.GetAllAsync();
            var venda = vendas.FirstOrDefault(v => v.ApartamentoId == apt.Id && v.ClienteId == cliente.Id);
            Assert.NotNull(venda);

            var updatedReserva = await uow.Reservas.GetByIdAsync(created.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusReserva.Confirmada, updatedReserva!.Status);

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
            await service.CancelAsync(created.Id);

            var updatedApt = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Disponivel, updatedApt!.Status);

            var updatedReserva = await uow.Reservas.GetByIdAsync(created.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusReserva.Cancelada, updatedReserva!.Status);

            context.Dispose();
        }
    }
}
