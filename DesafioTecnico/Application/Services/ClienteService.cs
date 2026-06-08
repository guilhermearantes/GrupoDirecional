using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DesafioTecnico.Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(IUnitOfWork uow, ILogger<ClienteService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

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
            _logger.LogInformation("Cliente criado: {ClienteId} — Email: {Email}", cliente.Id, cliente.Email);
            return cliente;
        }

        public async Task UpdateAsync(Cliente cliente, CancellationToken ct = default)
        {
            await _uow.Clientes.UpdateAsync(cliente, ct);
            await _uow.CommitAsync(ct);
            _logger.LogInformation("Cliente atualizado: {ClienteId}", cliente.Id);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _uow.Clientes.DeleteAsync(id, ct);
            await _uow.CommitAsync(ct);
            _logger.LogInformation("Cliente removido: {ClienteId}", id);
        }
    }
}
