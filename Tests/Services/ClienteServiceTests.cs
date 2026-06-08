using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Application.Services;

namespace Tests.Services
{
    public class ClienteServiceTests
    {
        private static (AppDbContext context, UnitOfWork uow, ClienteService service) BuildSut(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var context = new AppDbContext(options);
            var uow = new UnitOfWork(context);
            var service = new ClienteService(uow, NullLogger<ClienteService>.Instance);
            return (context, uow, service);
        }

        [Fact]
        public async Task Criar_DevePersistirCliente()
        {
            var (_, _, service) = BuildSut("TestDb_CreateCliente");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            await service.CreateAsync(cliente);

            var fetched = await service.GetByIdAsync(cliente.Id);
            Assert.NotNull(fetched);
            Assert.Equal(cliente.Email, fetched.Email);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNull_QuandoNaoEncontrado()
        {
            var (_, _, service) = BuildSut("TestDb_GetCliente_NotFound");

            var fetched = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(fetched);
        }

        [Fact]
        public async Task Atualizar_DevePersistirAlteracoes()
        {
            var (context, _, service) = BuildSut("TestDb_UpdateCliente");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            context.Clientes.Add(cliente);
            context.SaveChanges();

            cliente.Nome = "Nome Atualizado";
            await service.UpdateAsync(cliente);

            var fetched = await service.GetByIdAsync(cliente.Id);
            Assert.Equal("Nome Atualizado", fetched!.Nome);
        }

        [Fact]
        public async Task Excluir_DeveRemoverCliente()
        {
            var (context, _, service) = BuildSut("TestDb_DeleteCliente");

            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            context.Clientes.Add(cliente);
            context.SaveChanges();

            await service.DeleteAsync(cliente.Id);

            var fetched = await service.GetByIdAsync(cliente.Id);
            Assert.Null(fetched);
        }
    }
}
