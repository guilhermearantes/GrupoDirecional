using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;
using DesafioTecnico.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DesafioTecnico.Infrastructure.Services
{
    public class VendaService : IVendaService
    {
        private readonly AppDbContext _context;
        private readonly IVendaRepository _vendaRepo;
        private readonly IApartamentoRepository _apartRepo;
        private readonly ILogger<VendaService> _logger;

        public VendaService(AppDbContext context, IVendaRepository vendaRepo, IApartamentoRepository apartRepo, ILogger<VendaService> logger)
        {
            _context = context;
            _vendaRepo = vendaRepo;
            _apartRepo = apartRepo;
            _logger = logger;
        }

        public Task<IEnumerable<Venda>> GetAllAsync(CancellationToken ct = default)
            => _vendaRepo.GetAllAsync(ct);

        public Task<Venda?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _vendaRepo.GetByIdAsync(id, ct);

        public async Task<Venda> CreateAsync(Venda venda, CancellationToken ct = default)
        {
            var apt = await _apartRepo.GetByIdAsync(venda.ApartamentoId, ct)
                ?? throw new InvalidOperationException("Apartamento não encontrado.");

            apt.VenderDiretamente();

            venda.Id = Guid.NewGuid();
            venda.DataVenda = DateTime.UtcNow;

            if (_context.Database.IsRelational())
            {
                using var trx = await _context.Database.BeginTransactionAsync(ct);
                try
                {
                    await _vendaRepo.AddAsync(venda, ct);
                    await _apartRepo.UpdateAsync(apt, ct);
                    await trx.CommitAsync(ct);
                }
                catch
                {
                    await trx.RollbackAsync(ct);
                    throw;
                }
            }
            else
            {
                await _vendaRepo.AddAsync(venda, ct);
                await _apartRepo.UpdateAsync(apt, ct);
            }

            _logger.LogInformation("Venda {VendaId} criada para apartamento {ApartamentoId}", venda.Id, venda.ApartamentoId);
            return venda;
        }

        public Task UpdateAsync(Venda venda, CancellationToken ct = default)
            => _vendaRepo.UpdateAsync(venda, ct);

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
            => _vendaRepo.DeleteAsync(id, ct);
    }
}
