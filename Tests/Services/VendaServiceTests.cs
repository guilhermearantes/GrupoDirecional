using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Services
{
    public class VendaServiceTests
    {
        [Fact]
        public async Task Criar_DeveMudarApartamentoParaVendido_QuandoDisponivel()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateVenda")
                .Options;

            using var context = new AppDbContext(options);

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var uow = new UnitOfWork(context);
            var service = new VendaService(uow, NullLogger<VendaService>.Instance);

            var venda = Fixtures.FakeDataBuilder.CreateVenda(cliente.Id, apt.Id);
            var created = await service.CreateAsync(venda);

            var updatedApt = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido, updatedApt!.Status);
            Assert.Equal(venda.Id, created.Id);
        }

        [Theory]
        [InlineData(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido)]
        [InlineData(DesafioTecnico.Domain.Enums.StatusApartamento.Reservado)]
        public async Task Criar_DeveLancarExcecao_QuandoApartamentoNaoDisponivel(DesafioTecnico.Domain.Enums.StatusApartamento status)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_CreateVenda_NotAvailable_{status}")
                .Options;

            using var context = new AppDbContext(options);

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            apt.Status = status;
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var uow = new UnitOfWork(context);
            var service = new VendaService(uow, NullLogger<VendaService>.Instance);

            var venda = Fixtures.FakeDataBuilder.CreateVenda(cliente.Id, apt.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.CreateAsync(venda));
        }
    }
}
