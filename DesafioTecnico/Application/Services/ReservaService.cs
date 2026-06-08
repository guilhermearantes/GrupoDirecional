using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Factories;
using DesafioTecnico.Domain.Results;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DesafioTecnico.Application.Services
{
    public class ReservaService : IReservaService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(IUnitOfWork uow, ILogger<ReservaService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public Task<IEnumerable<Reserva>> GetAllAsync(CancellationToken ct = default)
            => _uow.Reservas.GetAllAsync(ct);

        public Task<Reserva?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _uow.Reservas.GetByIdAsync(id, ct);

        public async Task<(IEnumerable<Reserva> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var skip = (page - 1) * pageSize;
            var total = await _uow.Reservas.CountAsync(ct);
            var items = await _uow.Reservas.GetPagedAsync(skip, pageSize, ct);
            return (items, total);
        }

        public async Task<Result<Reserva>> CreateAsync(Reserva reserva, CancellationToken ct = default)
        {
            var cliente = await _uow.Clientes.GetByIdAsync(reserva.ClienteId, ct);
            if (cliente == null) return Result.Fail<Reserva>("Cliente não encontrado.");

            var apt = await _uow.Apartamentos.GetByIdAsync(reserva.ApartamentoId, ct);
            if (apt == null) return Result.Fail<Reserva>("Apartamento não encontrado.");

            var reservar = apt.Reservar();
            if (reservar.IsFailure) return Result.Fail<Reserva>(reservar.Error);

            reserva.Iniciar();
            await _uow.Reservas.AddAsync(reserva, ct);
            await _uow.Apartamentos.UpdateAsync(apt, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Reserva {ReservaId} criada para apartamento {ApartamentoId}", reserva.Id, reserva.ApartamentoId);
            return Result.Ok(reserva);
        }

        public async Task<Result<Guid>> ConfirmAsync(Guid id, CancellationToken ct = default)
        {
            var reserva = await _uow.Reservas.GetByIdAsync(id, ct);
            if (reserva == null) return Result.NotFound<Guid>("Reserva não encontrada.");

            var apt = await _uow.Apartamentos.GetByIdAsync(reserva.ApartamentoId, ct);
            if (apt == null) return Result.Fail<Guid>("Apartamento da reserva não encontrado.");

            var confirmar = reserva.Confirmar();
            if (confirmar.IsFailure) return Result.Fail<Guid>(confirmar.Error);

            var vender = apt.Vender();
            if (vender.IsFailure) return Result.Fail<Guid>(vender.Error);

            var venda = VendaFactory.CriarPorReserva(reserva, apt);

            await _uow.Vendas.AddAsync(venda, ct);
            await _uow.Apartamentos.UpdateAsync(apt, ct);
            await _uow.Reservas.UpdateAsync(reserva, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Reserva {ReservaId} confirmada — Venda {VendaId} gerada", id, venda.Id);
            return Result.Ok(venda.Id);
        }

        public async Task<Result> CancelAsync(Guid id, CancellationToken ct = default)
        {
            var reserva = await _uow.Reservas.GetByIdAsync(id, ct);
            if (reserva == null) return Result.NotFound("Reserva não encontrada.");

            var apt = await _uow.Apartamentos.GetByIdAsync(reserva.ApartamentoId, ct);
            if (apt == null) return Result.Fail("Apartamento da reserva não encontrado.");

            var cancelar = reserva.Cancelar();
            if (cancelar.IsFailure) return cancelar;

            var liberar = apt.Liberar();
            if (liberar.IsFailure) return liberar;

            await _uow.Reservas.UpdateAsync(reserva, ct);
            await _uow.Apartamentos.UpdateAsync(apt, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Reserva {ReservaId} cancelada", id);
            return Result.Ok();
        }

        public async Task UpdateAsync(Reserva reserva, CancellationToken ct = default)
        {
            await _uow.Reservas.UpdateAsync(reserva, ct);
            await _uow.CommitAsync(ct);
        }

        public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var reserva = await _uow.Reservas.GetByIdAsync(id, ct);
            if (reserva == null) return Result.NotFound("Reserva não encontrada.");

            if (reserva.Status == Domain.Enums.StatusReserva.Confirmada)
                return Result.Fail("Não é possível excluir uma reserva confirmada. Cancele a venda associada primeiro.");

            if (reserva.Status == Domain.Enums.StatusReserva.Pendente)
            {
                var apt = await _uow.Apartamentos.GetByIdAsync(reserva.ApartamentoId, ct);
                if (apt != null)
                {
                    var liberar = apt.Liberar();
                    if (liberar.IsFailure)
                        _logger.LogWarning("Apartamento {AptId} não pôde ser liberado ao excluir Reserva {ReservaId}: {Error}", reserva.ApartamentoId, id, liberar.Error);
                    else
                        await _uow.Apartamentos.UpdateAsync(apt, ct);
                }
            }

            await _uow.Reservas.DeleteAsync(id, ct);
            await _uow.CommitAsync(ct);
            _logger.LogInformation("Reserva {ReservaId} removida", id);
            return Result.Ok();
        }
    }
}
