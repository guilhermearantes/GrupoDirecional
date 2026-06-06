using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infraestructure.Repositories.Interfaces;
using DesafioTecnico.Infraestructure.Services.Interfaces;

namespace DesafioTecnico.Infraestructure.Services
{
    public class ApartamentoService : IApartamentoService
    {
        private readonly IApartamentoRepository _repo;

        public ApartamentoService(IApartamentoRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<Apartamento>> GetAllAsync() => _repo.GetAllAsync();

        public Task<Apartamento?> GetByIdAsync(Guid id) => _repo.GetByIdAsync(id);

        public async Task<Apartamento> CreateAsync(Apartamento apt)
        {
            apt.Id = Guid.NewGuid();
            apt.Status = Domain.Enums.StatusApartamento.Disponivel;
            await _repo.AddAsync(apt);
            return apt;
        }

        public Task UpdateAsync(Apartamento apt) => _repo.UpdateAsync(apt);

        public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);
    }
}

