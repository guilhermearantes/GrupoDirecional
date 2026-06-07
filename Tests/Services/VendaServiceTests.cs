using Microsoft.EntityFrameworkCore;

namespace Tests.Services
{
    public class VendaServiceTests
    {
        [Fact]
        public async Task CreateVenda_MarksApartmentAsVendido()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infrastructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateVenda")
                .Options;

            using var context = new DesafioTecnico.Infrastructure.Data.AppDbContext(options);

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento(); // Status = Disponivel
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var vendaRepo = new DesafioTecnico.Infrastructure.Repositories.VendaRepository(context);
            var apartRepo = new DesafioTecnico.Infrastructure.Repositories.ApartamentoRepository(context);
            var vendaService = new DesafioTecnico.Infrastructure.Services.VendaService(context, vendaRepo, apartRepo, Microsoft.Extensions.Logging.Abstractions.NullLogger<DesafioTecnico.Infrastructure.Services.VendaService>.Instance);

            var venda = Fixtures.FakeDataBuilder.CreateVenda(cliente.Id, apt.Id);

            var created = await vendaService.CreateAsync(venda);

            var updatedApt = await apartRepo.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido, updatedApt.Status);
            Assert.Equal(venda.Id, created.Id);
        }

        [Theory]
        [InlineData(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido)]
        [InlineData(DesafioTecnico.Domain.Enums.StatusApartamento.Reservado)]
        public async Task CreateVenda_ThrowsWhenApartmentNotDisponivel(DesafioTecnico.Domain.Enums.StatusApartamento status)
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infrastructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_CreateVenda_NotAvailable_{status}")
                .Options;

            using var context = new DesafioTecnico.Infrastructure.Data.AppDbContext(options);

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            apt.Status = status;
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var vendaRepo = new DesafioTecnico.Infrastructure.Repositories.VendaRepository(context);
            var apartRepo = new DesafioTecnico.Infrastructure.Repositories.ApartamentoRepository(context);
            var vendaService = new DesafioTecnico.Infrastructure.Services.VendaService(context, vendaRepo, apartRepo, Microsoft.Extensions.Logging.Abstractions.NullLogger<DesafioTecnico.Infrastructure.Services.VendaService>.Instance);

            var venda = Fixtures.FakeDataBuilder.CreateVenda(cliente.Id, apt.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await vendaService.CreateAsync(venda));
        }
    }
}
