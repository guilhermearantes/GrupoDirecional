using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Services;

namespace Tests.Services
{
    public class ApartamentoServiceMoreTests
    {
        [Fact]
        public async Task Atualizar_DevePersistirAlteracoes()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_UpdateApartamento")
                .Options;

            using var context = new AppDbContext(options);
            var uow = new UnitOfWork(context);
            var service = new ApartamentoService(uow);

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
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_DeleteApartamento")
                .Options;

            using var context = new AppDbContext(options);
            var uow = new UnitOfWork(context);
            var service = new ApartamentoService(uow);

            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            await service.DeleteAsync(apt.Id);

            var fetched = await uow.Apartamentos.GetByIdAsync(apt.Id);
            Assert.Null(fetched);
        }

        [Fact]
        public async Task Atualizar_NaoExistente_DeveLancarExcecao()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_UpdateNonexistent")
                .Options;

            using var context = new AppDbContext(options);
            var uow = new UnitOfWork(context);
            var service = new ApartamentoService(uow);

            var apt = Fixtures.FakeDataBuilder.CreateApartamento();

            await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException>(
                async () => await service.UpdateAsync(apt));
        }
    }
}
