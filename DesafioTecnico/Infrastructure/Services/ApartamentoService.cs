using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Enums;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Services.Interfaces;

namespace DesafioTecnico.Infrastructure.Services
{
    public class ApartamentoService : IApartamentoService
    {
        private readonly IUnitOfWork _uow;

        public ApartamentoService(IUnitOfWork uow) => _uow = uow;

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
            apt.Status = Domain.Enums.StatusApartamento.Disponivel;
            await _uow.Apartamentos.AddAsync(apt, ct);
            await _uow.CommitAsync(ct);
            return apt;
        }

        public async Task UpdateAsync(Apartamento apt, CancellationToken ct = default)
        {
            await _uow.Apartamentos.UpdateAsync(apt, ct);
            await _uow.CommitAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _uow.Apartamentos.DeleteAsync(id, ct);
            await _uow.CommitAsync(ct);
        }
    }
}
