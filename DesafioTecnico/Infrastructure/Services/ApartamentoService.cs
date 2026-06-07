using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;
using DesafioTecnico.Infrastructure.Services.Interfaces;

namespace DesafioTecnico.Infrastructure.Services
{
    public class ApartamentoService : IApartamentoService
    {
        private readonly IApartamentoRepository _repo;

        public ApartamentoService(IApartamentoRepository repo) => _repo = repo;

        public Task<IEnumerable<Apartamento>> GetAllAsync(CancellationToken ct = default)
            => _repo.GetAllAsync(ct);

        public Task<Apartamento?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _repo.GetByIdAsync(id, ct);

        public async Task<Apartamento> CreateAsync(Apartamento apt, CancellationToken ct = default)
        {
            apt.Id = Guid.NewGuid();
            apt.Status = Domain.Enums.StatusApartamento.Disponivel;
            await _repo.AddAsync(apt, ct);
            return apt;
        }

        public Task UpdateAsync(Apartamento apt, CancellationToken ct = default)
            => _repo.UpdateAsync(apt, ct);

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
            => _repo.DeleteAsync(id, ct);
    }
}
