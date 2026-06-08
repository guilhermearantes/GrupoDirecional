using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Factories;
using DesafioTecnico.Domain.Results;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DesafioTecnico.Application.Services
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

        public async Task<(IEnumerable<Venda> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var skip = (page - 1) * pageSize;
            var total = await _uow.Vendas.CountAsync(ct);
            var items = await _uow.Vendas.GetPagedAsync(skip, pageSize, ct);
            return (items, total);
        }

        public async Task<Result<Venda>> CreateAsync(Venda venda, CancellationToken ct = default)
        {
            var cliente = await _uow.Clientes.GetByIdAsync(venda.ClienteId, ct);
            if (cliente == null) return Result.Fail<Venda>("Cliente não encontrado.");

            var apt = await _uow.Apartamentos.GetByIdAsync(venda.ApartamentoId, ct);
            if (apt == null) return Result.Fail<Venda>("Apartamento não encontrado.");

            var vender = apt.VenderDiretamente();
            if (vender.IsFailure) return Result.Fail<Venda>(vender.Error);

            var novaVenda = VendaFactory.CriarVendaDireta(venda.ClienteId, venda.ApartamentoId, venda.ValorPago);

            await _uow.Vendas.AddAsync(novaVenda, ct);
            await _uow.Apartamentos.UpdateAsync(apt, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Venda {VendaId} criada para apartamento {ApartamentoId}", novaVenda.Id, novaVenda.ApartamentoId);
            return Result.Ok(novaVenda);
        }

        public async Task UpdateAsync(Venda venda, CancellationToken ct = default)
        {
            await _uow.Vendas.UpdateAsync(venda, ct);
            await _uow.CommitAsync(ct);
        }

        public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var venda = await _uow.Vendas.GetByIdAsync(id, ct);
            if (venda == null) return Result.NotFound("Venda não encontrada.");

            var apt = await _uow.Apartamentos.GetByIdAsync(venda.ApartamentoId, ct);
            if (apt != null)
            {
                var estornar = apt.EstornarVenda();
                if (estornar.IsFailure)
                    _logger.LogWarning("Apartamento {AptId} não estava Vendido ao remover Venda {VendaId}: {Error}", venda.ApartamentoId, id, estornar.Error);
                else
                    await _uow.Apartamentos.UpdateAsync(apt, ct);
            }

            await _uow.Vendas.DeleteAsync(id, ct);
            await _uow.CommitAsync(ct);
            _logger.LogInformation("Venda {VendaId} removida", id);
            return Result.Ok();
        }
    }
}
