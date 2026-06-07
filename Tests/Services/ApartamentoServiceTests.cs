using Microsoft.EntityFrameworkCore;

namespace Tests.Services
{
    public class ApartamentoServiceTests
    {
        [Fact]
        public async Task CreateApartamento_SetsDefaults_AndPersists()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infrastructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateApartamento")
                .Options;

            using var context = new DesafioTecnico.Infrastructure.Data.AppDbContext(options);

            var repo = new DesafioTecnico.Infrastructure.Repositories.ApartamentoRepository(context);
            var service = new DesafioTecnico.Infrastructure.Services.ApartamentoService(repo);

            var apt = Tests.Fixtures.FakeDataBuilder.CreateApartamento();

            var created = await service.CreateAsync(apt);

            var fetched = await repo.GetByIdAsync(created.Id);
            Assert.NotNull(fetched);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Disponivel, fetched.Status);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotFound()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infrastructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Apartamento_NotFound")
                .Options;

            using var context = new DesafioTecnico.Infrastructure.Data.AppDbContext(options);

            var repo = new DesafioTecnico.Infrastructure.Repositories.ApartamentoRepository(context);
            var service = new DesafioTecnico.Infrastructure.Services.ApartamentoService(repo);

            var fetched = await service.GetByIdAsync(Guid.NewGuid());
            Assert.Null(fetched);
        }
    }
}
