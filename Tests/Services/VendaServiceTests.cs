using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Application.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Services
{
    public class VendaServiceTests
    {
        private static (AppDbContext context, UnitOfWork uow, VendaService service) BuildSut(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var context = new AppDbContext(options);
            var uow = new UnitOfWork(context);
            var service = new VendaService(uow, NullLogger<VendaService>.Instance);
            return (context, uow, service);
        }

        [Fact]
        public async Task Criar_DeveMudarApartamentoParaVendido_QuandoDisponivel()
        {
            var (context, uow, service) = BuildSut("TestDb_CreateVenda");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var venda = Fixtures.FakeDataBuilder.CreateVenda(cliente.Id, apt.Id);
            var result = await service.CreateAsync(venda);

            Assert.True(result.IsSuccess);
            var updatedApt = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido, updatedApt!.Status);
            Assert.NotEqual(Guid.Empty, result.Value.Id);
        }

        [Fact]
        public async Task Criar_DeveRetornarFalha_QuandoClienteNaoEncontrado()
        {
            var (context, _, service) = BuildSut("TestDb_CreateVenda_ClienteNotFound");

            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var venda = Fixtures.FakeDataBuilder.CreateVenda(Guid.NewGuid(), apt.Id);
            var result = await service.CreateAsync(venda);

            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
            context.Dispose();
        }

        [Theory]
        [InlineData(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido)]
        [InlineData(DesafioTecnico.Domain.Enums.StatusApartamento.Reservado)]
        public async Task Criar_DeveRetornarFalha_QuandoApartamentoNaoDisponivel(DesafioTecnico.Domain.Enums.StatusApartamento status)
        {
            var (context, _, service) = BuildSut($"TestDb_CreateVenda_NotAvailable_{status}");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            apt.Status = status;
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var venda = Fixtures.FakeDataBuilder.CreateVenda(cliente.Id, apt.Id);
            var result = await service.CreateAsync(venda);

            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
        }

        [Fact]
        public async Task Atualizar_DevePersistirNovoValorPago()
        {
            var (context, uow, service) = BuildSut("TestDb_UpdateVenda");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var venda = Fixtures.FakeDataBuilder.CreateVenda(cliente.Id, apt.Id);
            context.Vendas.Add(venda);
            context.SaveChanges();

            venda.ValorPago = 500000m;
            await service.UpdateAsync(venda);

            var fetched = await uow.Vendas.GetByIdAsync(venda.Id);
            Assert.Equal(500000m, fetched!.ValorPago);
        }

        [Fact]
        public async Task Excluir_DeveRemoverVenda()
        {
            var (context, uow, service) = BuildSut("TestDb_DeleteVenda");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var venda = Fixtures.FakeDataBuilder.CreateVenda(cliente.Id, apt.Id);
            context.Vendas.Add(venda);
            context.SaveChanges();

            await service.DeleteAsync(venda.Id);

            var fetched = await uow.Vendas.GetByIdAsync(venda.Id);
            Assert.Null(fetched);
        }
    }
}
