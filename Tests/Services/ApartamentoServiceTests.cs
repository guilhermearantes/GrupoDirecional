using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Services;

namespace Tests.Services
{
    public class ApartamentoServiceTests
    {
        [Fact]
        public async Task Criar_DeveDefinirStatusDisponivel_EPersistir()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateApartamento")
                .Options;

            using var context = new AppDbContext(options);
            var uow = new UnitOfWork(context);
            var service = new ApartamentoService(uow);

            var apt = Tests.Fixtures.FakeDataBuilder.CreateApartamento();

            var created = await service.CreateAsync(apt);

            var fetched = await uow.Apartamentos.GetByIdAsync(created.Id);
            Assert.NotNull(fetched);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Disponivel, fetched.Status);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNull_QuandoNaoEncontrado()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Apartamento_NotFound")
                .Options;

            using var context = new AppDbContext(options);
            var uow = new UnitOfWork(context);
            var service = new ApartamentoService(uow);

            var fetched = await service.GetByIdAsync(Guid.NewGuid());
            Assert.Null(fetched);
        }
    }
}
