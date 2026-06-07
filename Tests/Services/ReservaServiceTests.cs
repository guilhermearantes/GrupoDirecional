using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Tests.Services
{
    public class ReservaServiceTests
    {
        [Fact]
        public async Task CreateReserva_SetsApartmentToReservado()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infrastructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateReserva")
                .Options;

            using var context = new DesafioTecnico.Infrastructure.Data.AppDbContext(options);

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var reservaRepo = new DesafioTecnico.Infrastructure.Repositories.ReservaRepository(context);
            var apartRepo = new DesafioTecnico.Infrastructure.Repositories.ApartamentoRepository(context);
            var vendaRepo = new DesafioTecnico.Infrastructure.Repositories.VendaRepository(context);
            var reservaService = new DesafioTecnico.Infrastructure.Services.ReservaService(context, reservaRepo, apartRepo, vendaRepo, Microsoft.Extensions.Logging.Abstractions.NullLogger<DesafioTecnico.Infrastructure.Services.ReservaService>.Instance);

            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                ClienteId = cliente.Id,
                ApartamentoId = apt.Id
            };

            var created = await reservaService.CreateAsync(reserva);

            var updatedApt = await apartRepo.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Reservado, updatedApt.Status);
            Assert.Equal(reserva.Id, created.Id);
        }

        [Fact]
        public async Task CreateReserva_ThrowsWhenApartmentNotAvailable()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infrastructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateReserva_NotAvailable")
                .Options;

            using var context = new DesafioTecnico.Infrastructure.Data.AppDbContext(options);

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            apt.Status = DesafioTecnico.Domain.Enums.StatusApartamento.Vendido;
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var reservaRepo = new DesafioTecnico.Infrastructure.Repositories.ReservaRepository(context);
            var apartRepo = new DesafioTecnico.Infrastructure.Repositories.ApartamentoRepository(context);
            var vendaRepo = new DesafioTecnico.Infrastructure.Repositories.VendaRepository(context);
            var reservaService = new DesafioTecnico.Infrastructure.Services.ReservaService(context, reservaRepo, apartRepo, vendaRepo, Microsoft.Extensions.Logging.Abstractions.NullLogger<DesafioTecnico.Infrastructure.Services.ReservaService>.Instance);

            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                ClienteId = cliente.Id,
                ApartamentoId = apt.Id
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await reservaService.CreateAsync(reserva));
        }

        [Fact]
        public async Task ConfirmReserva_ThrowsWhenReservaNotFound()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infrastructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_ConfirmReserva_NotFound")
                .Options;

            using var context = new DesafioTecnico.Infrastructure.Data.AppDbContext(options);

            var reservaRepo = new DesafioTecnico.Infrastructure.Repositories.ReservaRepository(context);
            var apartRepo = new DesafioTecnico.Infrastructure.Repositories.ApartamentoRepository(context);
            var vendaRepo = new DesafioTecnico.Infrastructure.Repositories.VendaRepository(context);
            var reservaService = new DesafioTecnico.Infrastructure.Services.ReservaService(context, reservaRepo, apartRepo, vendaRepo, Microsoft.Extensions.Logging.Abstractions.NullLogger<DesafioTecnico.Infrastructure.Services.ReservaService>.Instance);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await reservaService.ConfirmAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task ConfirmReserva_ThrowsWhenNotPending()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infrastructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_ConfirmReserva_NotPending")
                .Options;

            using var context = new DesafioTecnico.Infrastructure.Data.AppDbContext(options);

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

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

            var reservaRepo = new DesafioTecnico.Infrastructure.Repositories.ReservaRepository(context);
            var apartRepo = new DesafioTecnico.Infrastructure.Repositories.ApartamentoRepository(context);
            var vendaRepo = new DesafioTecnico.Infrastructure.Repositories.VendaRepository(context);
            var reservaService = new DesafioTecnico.Infrastructure.Services.ReservaService(context, reservaRepo, apartRepo, vendaRepo, Microsoft.Extensions.Logging.Abstractions.NullLogger<DesafioTecnico.Infrastructure.Services.ReservaService>.Instance);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await reservaService.ConfirmAsync(reserva.Id));
        }

        [Fact]
        public async Task CancelReserva_ThrowsWhenNotPending()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infrastructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CancelReserva_NotPending")
                .Options;

            using var context = new DesafioTecnico.Infrastructure.Data.AppDbContext(options);

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

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

            var reservaRepo = new DesafioTecnico.Infrastructure.Repositories.ReservaRepository(context);
            var apartRepo = new DesafioTecnico.Infrastructure.Repositories.ApartamentoRepository(context);
            var vendaRepo = new DesafioTecnico.Infrastructure.Repositories.VendaRepository(context);
            var reservaService = new DesafioTecnico.Infrastructure.Services.ReservaService(context, reservaRepo, apartRepo, vendaRepo, Microsoft.Extensions.Logging.Abstractions.NullLogger<DesafioTecnico.Infrastructure.Services.ReservaService>.Instance);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await reservaService.CancelAsync(reserva.Id));
        }

        [Fact]
        public async Task ConfirmReserva_CreatesVenda_And_MarksApartamentoVendido()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infrastructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_ConfirmReserva")
                .Options;

            using var context = new DesafioTecnico.Infrastructure.Data.AppDbContext(options);

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var reservaRepo = new DesafioTecnico.Infrastructure.Repositories.ReservaRepository(context);
            var apartRepo = new DesafioTecnico.Infrastructure.Repositories.ApartamentoRepository(context);
            var vendaRepo = new DesafioTecnico.Infrastructure.Repositories.VendaRepository(context);
            var reservaService = new DesafioTecnico.Infrastructure.Services.ReservaService(context, reservaRepo, apartRepo, vendaRepo, Microsoft.Extensions.Logging.Abstractions.NullLogger<DesafioTecnico.Infrastructure.Services.ReservaService>.Instance);

            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                ClienteId = cliente.Id,
                ApartamentoId = apt.Id
            };

            var created = await reservaService.CreateAsync(reserva);

            await reservaService.ConfirmAsync(created.Id);

            var updatedApt = await apartRepo.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido, updatedApt.Status);

            var venda = (await vendaRepo.GetAllAsync()).FirstOrDefault(v => v.ApartamentoId == apt.Id && v.ClienteId == cliente.Id);
            Assert.NotNull(venda);

            var updatedReserva = await reservaRepo.GetByIdAsync(created.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusReserva.Confirmada, updatedReserva.Status);
        }

        [Fact]
        public async Task CancelReserva_SetsApartmentToDisponivel_And_StatusCancelada()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infrastructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CancelReserva")
                .Options;

            using var context = new DesafioTecnico.Infrastructure.Data.AppDbContext(options);

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var reservaRepo = new DesafioTecnico.Infrastructure.Repositories.ReservaRepository(context);
            var apartRepo = new DesafioTecnico.Infrastructure.Repositories.ApartamentoRepository(context);
            var vendaRepo = new DesafioTecnico.Infrastructure.Repositories.VendaRepository(context);
            var reservaService = new DesafioTecnico.Infrastructure.Services.ReservaService(context, reservaRepo, apartRepo, vendaRepo, Microsoft.Extensions.Logging.Abstractions.NullLogger<DesafioTecnico.Infrastructure.Services.ReservaService>.Instance);

            var reserva = new DesafioTecnico.Domain.Entities.Reserva
            {
                ClienteId = cliente.Id,
                ApartamentoId = apt.Id
            };

            var created = await reservaService.CreateAsync(reserva);

            await reservaService.CancelAsync(created.Id);

            var updatedApt = await apartRepo.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Disponivel, updatedApt.Status);

            var updatedReserva = await reservaRepo.GetByIdAsync(created.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusReserva.Cancelada, updatedReserva.Status);
        }
    }
}
