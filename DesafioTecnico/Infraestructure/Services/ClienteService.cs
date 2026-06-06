using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infraestructure.Repositories.Interfaces;
using DesafioTecnico.Infraestructure.Services.Interfaces;

namespace DesafioTecnico.Infraestructure.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _repository;

        public ClienteService(IClienteRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Cliente>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Cliente?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

        public async Task CreateAsync(Cliente cliente)
        {
            cliente.Id = Guid.NewGuid();
            await _repository.AddAsync(cliente);
        }

        public async Task UpdateAsync(Cliente cliente)
        {
            await _repository.UpdateAsync(cliente);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
