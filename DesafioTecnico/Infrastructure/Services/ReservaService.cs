using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DesafioTecnico.Infrastructure.Services
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

        public async Task<Reserva> CreateAsync(Reserva reserva, CancellationToken ct = default)
        {
            var apt = await _uow.Apartamentos.GetByIdAsync(reserva.ApartamentoId, ct)
                ?? throw new InvalidOperationException("Apartamento não encontrado.");

            apt.Reservar();
            reserva.Iniciar();

            await _uow.Reservas.AddAsync(reserva, ct);
            await _uow.Apartamentos.UpdateAsync(apt, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Reserva {ReservaId} criada para apartamento {ApartamentoId}", reserva.Id, reserva.ApartamentoId);
            return reserva;
        }

        public async Task ConfirmAsync(Guid id, CancellationToken ct = default)
        {
            var reserva = await _uow.Reservas.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("Reserva não encontrada.");
            var apt = await _uow.Apartamentos.GetByIdAsync(reserva.ApartamentoId, ct)
                ?? throw new InvalidOperationException("Apartamento da reserva não encontrado.");

            reserva.Confirmar();
            apt.Vender();

            var venda = new Venda
            {
                Id = Guid.NewGuid(),
                ClienteId = reserva.ClienteId,
                ApartamentoId = reserva.ApartamentoId,
                DataVenda = DateTime.UtcNow,
                ValorPago = apt.Valor
            };

            await _uow.Vendas.AddAsync(venda, ct);
            await _uow.Apartamentos.UpdateAsync(apt, ct);
            await _uow.Reservas.UpdateAsync(reserva, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Reserva {ReservaId} confirmada — Venda {VendaId} gerada", id, venda.Id);
        }

        public async Task CancelAsync(Guid id, CancellationToken ct = default)
        {
            var reserva = await _uow.Reservas.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("Reserva não encontrada.");
            var apt = await _uow.Apartamentos.GetByIdAsync(reserva.ApartamentoId, ct)
                ?? throw new InvalidOperationException("Apartamento da reserva não encontrado.");

            reserva.Cancelar();
            apt.Liberar();

            await _uow.Reservas.UpdateAsync(reserva, ct);
            await _uow.Apartamentos.UpdateAsync(apt, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Reserva {ReservaId} cancelada", id);
        }

        public Task UpdateAsync(Reserva reserva, CancellationToken ct = default)
            => _uow.Reservas.UpdateAsync(reserva, ct);

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
            => _uow.Reservas.DeleteAsync(id, ct);
    }
}
