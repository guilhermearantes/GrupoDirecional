using Microsoft.EntityFrameworkCore;

namespace Tests.Services
{
    public class ApartamentoServiceMoreTests
    {
        [Fact]
        public async Task UpdateApartamento_PersistsChanges()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infraestructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_UpdateApartamento")
                .Options;

            using var context = new DesafioTecnico.Infraestructure.Data.AppDbContext(options);

            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var repo = new DesafioTecnico.Infraestructure.Repositories.ApartamentoRepository(context);
            var service = new DesafioTecnico.Infraestructure.Services.ApartamentoService(repo);

            apt.Bloco = "B2";
            apt.Valor = 999999m;

            await service.UpdateAsync(apt);

            var fetched = await repo.GetByIdAsync(apt.Id);
            Assert.Equal("B2", fetched.Bloco);
            Assert.Equal(999999m, fetched.Valor);
        }

        [Fact]
        public async Task DeleteApartamento_RemovesEntity()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infraestructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_DeleteApartamento")
                .Options;

            using var context = new DesafioTecnico.Infraestructure.Data.AppDbContext(options);

            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            context.Apartamentos.Add(apt);
            context.SaveChanges();

            var repo = new DesafioTecnico.Infraestructure.Repositories.ApartamentoRepository(context);
            var service = new DesafioTecnico.Infraestructure.Services.ApartamentoService(repo);

            await service.DeleteAsync(apt.Id);

            var fetched = await repo.GetByIdAsync(apt.Id);
            Assert.Null(fetched);
        }

        [Fact]
        public async Task Update_Nonexistent_ThrowsOrNoop()
        {
            var options = new DbContextOptionsBuilder<DesafioTecnico.Infraestructure.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_UpdateNonexistent")
                .Options;

            using var context = new DesafioTecnico.Infraestructure.Data.AppDbContext(options);

            var repo = new DesafioTecnico.Infraestructure.Repositories.ApartamentoRepository(context);
            var service = new DesafioTecnico.Infraestructure.Services.ApartamentoService(repo);

            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            // do not add to context

            // ensure exception is thrown when trying to update an entity that does not exist in InMemory provider
            await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException>(async () => await service.UpdateAsync(apt));
        }
    }
}
