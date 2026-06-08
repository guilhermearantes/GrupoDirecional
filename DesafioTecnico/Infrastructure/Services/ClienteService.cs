using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Services.Interfaces;

namespace DesafioTecnico.Infrastructure.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IUnitOfWork _uow;

        public ClienteService(IUnitOfWork uow) => _uow = uow;

        public Task<IEnumerable<Cliente>> GetAllAsync(CancellationToken ct = default)
            => _uow.Clientes.GetAllAsync(ct);

        public Task<Cliente?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _uow.Clientes.GetByIdAsync(id, ct);

        public async Task<(IEnumerable<Cliente> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var skip = (page - 1) * pageSize;
            var total = await _uow.Clientes.CountAsync(ct);
            var items = await _uow.Clientes.GetPagedAsync(skip, pageSize, ct);
            return (items, total);
        }

        public async Task<Cliente> CreateAsync(Cliente cliente, CancellationToken ct = default)
        {
            cliente.Id = Guid.NewGuid();
            await _uow.Clientes.AddAsync(cliente, ct);
            await _uow.CommitAsync(ct);
            return cliente;
        }

        public async Task UpdateAsync(Cliente cliente, CancellationToken ct = default)
        {
            await _uow.Clientes.UpdateAsync(cliente, ct);
            await _uow.CommitAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _uow.Clientes.DeleteAsync(id, ct);
            await _uow.CommitAsync(ct);
        }
    }
}
