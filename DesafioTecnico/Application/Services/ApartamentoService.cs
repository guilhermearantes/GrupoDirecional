using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Enums;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DesafioTecnico.Application.Services
{
    public class ApartamentoService : IApartamentoService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ApartamentoService> _logger;

        public ApartamentoService(IUnitOfWork uow, ILogger<ApartamentoService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public Task<IEnumerable<Apartamento>> GetAllAsync(CancellationToken ct = default)
            => _uow.Apartamentos.GetAllAsync(ct);

        public Task<Apartamento?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _uow.Apartamentos.GetByIdAsync(id, ct);

        public async Task<(IEnumerable<Apartamento> Items, int Total)> GetPagedAsync(int page, int pageSize, StatusApartamento? status = null, CancellationToken ct = default)
        {
            var skip = (page - 1) * pageSize;
            var total = await _uow.Apartamentos.CountAsync(status, ct);
            var items = await _uow.Apartamentos.GetPagedAsync(skip, pageSize, status, ct);
            return (items, total);
        }

        public async Task<Apartamento> CreateAsync(Apartamento apt, CancellationToken ct = default)
        {
            apt.Id = Guid.NewGuid();
            apt.Status = StatusApartamento.Disponivel;
            await _uow.Apartamentos.AddAsync(apt, ct);
            await _uow.CommitAsync(ct);
            _logger.LogInformation("Apartamento criado: {ApartamentoId} — Código: {Codigo}", apt.Id, apt.Codigo);
            return apt;
        }

        public async Task UpdateAsync(Apartamento apt, CancellationToken ct = default)
        {
            await _uow.Apartamentos.UpdateAsync(apt, ct);
            await _uow.CommitAsync(ct);
            _logger.LogInformation("Apartamento atualizado: {ApartamentoId}", apt.Id);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _uow.Apartamentos.DeleteAsync(id, ct);
            await _uow.CommitAsync(ct);
            _logger.LogInformation("Apartamento removido: {ApartamentoId}", id);
        }
    }
}
