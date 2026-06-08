using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Services;

namespace Tests.Services
{
    public class ApartamentoServiceTests
    {
        private static (AppDbContext context, UnitOfWork uow, ApartamentoService service) BuildSut(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var context = new AppDbContext(options);
            var uow = new UnitOfWork(context);
            var service = new ApartamentoService(uow);
            return (context, uow, service);
        }

        [Fact]
        public async Task Criar_DeveDefinirStatusDisponivel_EPersistir()
        {
            var (_, uow, service) = BuildSut("TestDb_CreateApartamento");

            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            var created = await service.CreateAsync(apt);

            var fetched = await uow.Apartamentos.GetByIdAsync(created.Id);
            Assert.NotNull(fetched);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Disponivel, fetched.Status);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNull_QuandoNaoEncontrado()
        {
            var (_, _, service) = BuildSut("TestDb_Apartamento_NotFound");

            var fetched = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(fetched);
        }

        [Fact]
        public async Task Atualizar_DevePersistirAlteracoes()
        {
            var (context, uow, service) = BuildSut("TestDb_UpdateApartamento");

            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            apt.Bloco = "B2";
            apt.Valor = 999999m;
            await service.UpdateAsync(apt);

            var fetched = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Equal("B2", fetched!.Bloco);
            Assert.Equal(999999m, fetched.Valor);
        }

        [Fact]
        public async Task Excluir_DeveRemoverEntidade()
        {
            var (context, uow, service) = BuildSut("TestDb_DeleteApartamento");

            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            await service.DeleteAsync(apt.Id);

            var fetched = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Null(fetched);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarErro_QuandoEntidadeNaoExiste()
        {
            var (_, _, service) = BuildSut("TestDb_UpdateNonexistent");

            var apt = Fixtures.FakeDataBuilder.CreateApartamento();

            await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException>(
                async () => await service.UpdateAsync(apt));
        }
    }
}
