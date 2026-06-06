using Microsoft.EntityFrameworkCore;

namespace Tests.Services
{
    public class VendaServiceTests
    {
        [Fact]
        public async Task CreateVenda_MarksApartmentAsVendido()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infraestructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateVenda")
                .Options;

            using var context = new DesafioTecnico.Infraestructure.Data.AppDbContext(options);

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var vendaRepo = new DesafioTecnico.Infraestructure.Repositories.VendaRepository(context);
            var apartRepo = new DesafioTecnico.Infraestructure.Repositories.ApartamentoRepository(context);
            var vendaService = new DesafioTecnico.Infraestructure.Services.VendaService(context, vendaRepo, apartRepo);

            var venda = Fixtures.FakeDataBuilder.CreateVenda(cliente.Id, apt.Id);

            var created = await vendaService.CreateAsync(venda);

            var updatedApt = await apartRepo.GetByIdAsync(apt.Id);
            Xunit.Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido, updatedApt.Status);
            Xunit.Assert.Equal(venda.Id, created.Id);
        }

        [Fact]
        public async Task CreateVenda_ThrowsWhenApartmentNotAvailable()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infraestructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateVenda_NotAvailable")
                .Options;

            using var context = new DesafioTecnico.Infraestructure.Data.AppDbContext(options);

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            apt.Status = DesafioTecnico.Domain.Enums.StatusApartamento.Vendido;
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var vendaRepo = new DesafioTecnico.Infraestructure.Repositories.VendaRepository(context);
            var apartRepo = new DesafioTecnico.Infraestructure.Repositories.ApartamentoRepository(context);
            var vendaService = new DesafioTecnico.Infraestructure.Services.VendaService(context, vendaRepo, apartRepo);

            var venda = Fixtures.FakeDataBuilder.CreateVenda(cliente.Id, apt.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await vendaService.CreateAsync(venda));
        }
    }
}
