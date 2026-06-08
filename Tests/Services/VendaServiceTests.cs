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

            var result = await service.DeleteAsync(venda.Id);

            Assert.True(result.IsSuccess);
            var fetched = await uow.Vendas.GetByIdAsync(venda.Id);
            Assert.Null(fetched);
        }

        [Fact]
        public async Task Excluir_DeveRetornarNotFound_QuandoVendaNaoEncontrada()
        {
            var (_, _, service) = BuildSut("TestDb_DeleteVenda_NotFound");

            var result = await service.DeleteAsync(Guid.NewGuid());

            Assert.True(result.IsNotFound);
            Assert.NotEmpty(result.Error);
        }

        [Fact]
        public async Task Excluir_DeveRestaurarStatusApartamento_QuandoBemSucedido()
        {
            var (context, uow, service) = BuildSut("TestDb_DeleteVenda_RestoreApt");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Clientes.Add(cliente);
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var created = await service.CreateAsync(Fixtures.FakeDataBuilder.CreateVenda(cliente.Id, apt.Id));
            Assert.True(created.IsSuccess);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido,
                (await uow.Apartamentos.GetByIdAsync(apt.Id))!.Status);

            var result = await service.DeleteAsync(created.Value.Id);

            Assert.True(result.IsSuccess);
            Assert.Null(await uow.Vendas.GetByIdAsync(created.Value.Id));
            var restoredApt = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Disponivel, restoredApt!.Status);
        }
    }
}
