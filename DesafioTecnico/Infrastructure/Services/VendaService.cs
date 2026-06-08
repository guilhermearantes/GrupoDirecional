using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DesafioTecnico.Infrastructure.Services
{
    public class VendaService : IVendaService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<VendaService> _logger;

        public VendaService(IUnitOfWork uow, ILogger<VendaService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public Task<IEnumerable<Venda>> GetAllAsync(CancellationToken ct = default)
            => _uow.Vendas.GetAllAsync(ct);

        public Task<Venda?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _uow.Vendas.GetByIdAsync(id, ct);

        public async Task<Venda> CreateAsync(Venda venda, CancellationToken ct = default)
        {
            var apt = await _uow.Apartamentos.GetByIdAsync(venda.ApartamentoId, ct)
                ?? throw new InvalidOperationException("Apartamento não encontrado.");

            apt.VenderDiretamente();

            venda.Id = Guid.NewGuid();
            venda.DataVenda = DateTime.UtcNow;

            await _uow.Vendas.AddAsync(venda, ct);
            await _uow.Apartamentos.UpdateAsync(apt, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Venda {VendaId} criada para apartamento {ApartamentoId}", venda.Id, venda.ApartamentoId);
            return venda;
        }

        public async Task UpdateAsync(Venda venda, CancellationToken ct = default)
        {
            await _uow.Vendas.UpdateAsync(venda, ct);
            await _uow.CommitAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _uow.Vendas.DeleteAsync(id, ct);
            await _uow.CommitAsync(ct);
        }
    }
}
