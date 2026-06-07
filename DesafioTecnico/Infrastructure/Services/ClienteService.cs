using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;
using DesafioTecnico.Infrastructure.Services.Interfaces;

namespace DesafioTecnico.Infrastructure.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _repository;

        public ClienteService(IClienteRepository repository) => _repository = repository;

        public Task<IEnumerable<Cliente>> GetAllAsync(CancellationToken ct = default)
            => _repository.GetAllAsync(ct);

        public Task<Cliente?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _repository.GetByIdAsync(id, ct);

        public async Task CreateAsync(Cliente cliente, CancellationToken ct = default)
        {
            cliente.Id = Guid.NewGuid();
            await _repository.AddAsync(cliente, ct);
        }

        public Task UpdateAsync(Cliente cliente, CancellationToken ct = default)
            => _repository.UpdateAsync(cliente, ct);

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
            => _repository.DeleteAsync(id, ct);
    }
}
