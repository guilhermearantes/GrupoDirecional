using Microsoft.EntityFrameworkCore;

namespace Tests.Services
{
    public class ClienteServiceTests
    {
        [Fact]
        public async Task CreateAndGetCliente_Works()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infraestructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestClienteDb")
                .Options;

            using var context = new DesafioTecnico.Infraestructure.Data.AppDbContext(options);

            var cliente = Tests.Fixtures.FakeDataBuilder.CreateCliente();

            var repo = new DesafioTecnico.Infraestructure.Repositories.ClienteRepository(context);
            var service = new DesafioTecnico.Infraestructure.Services.ClienteService(repo);

            await service.CreateAsync(cliente);

            var fetched = await service.GetByIdAsync(cliente.Id);
            Assert.NotNull(fetched);
            Assert.Equal(cliente.Email, fetched.Email);
        }
    }
}
