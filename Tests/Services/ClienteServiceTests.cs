using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Services;

namespace Tests.Services
{
    public class ClienteServiceTests
    {
        [Fact]
        public async Task CriarEObterCliente_DeveRetornarClientePersistido()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestClienteDb")
                .Options;

            using var context = new AppDbContext(options);
            var uow = new UnitOfWork(context);
            var service = new ClienteService(uow);

            var cliente = Tests.Fixtures.FakeDataBuilder.CreateCliente();

            await service.CreateAsync(cliente);

            var fetched = await service.GetByIdAsync(cliente.Id);
            Assert.NotNull(fetched);
            Assert.Equal(cliente.Email, fetched.Email);
        }
    }
}
