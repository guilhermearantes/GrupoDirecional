namespace DesafioTecnico.Infrastructure.Services
{
    using DesafioTecnico.Infrastructure.Repositories.Interfaces;
    using DesafioTecnico.Domain.Entities;
    using DesafioTecnico.Infrastructure.Data;
    using DesafioTecnico.Infrastructure.Services.Interfaces;
    using Microsoft.Extensions.Logging;

    public class ReservaService : IReservaService
    {
        private readonly AppDbContext _context;
        private readonly IReservaRepository _reservaRepo;
        private readonly IApartamentoRepository _apartRepo;
        private readonly IVendaRepository _vendaRepo;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(AppDbContext context, IReservaRepository reservaRepo, IApartamentoRepository apartRepo, IVendaRepository vendaRepo, ILogger<ReservaService> logger)
        {
            _context = context;
            _reservaRepo = reservaRepo;
            _apartRepo = apartRepo;
            _vendaRepo = vendaRepo;
            _logger = logger;
        }

        public Task<IEnumerable<Reserva>> GetAllAsync(CancellationToken ct = default)
            => _reservaRepo.GetAllAsync(ct);

        public Task<Reserva?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _reservaRepo.GetByIdAsync(id, ct);

        public async Task<Reserva> CreateAsync(Reserva reserva, CancellationToken ct = default)
        {
            var apt = await _apartRepo.GetByIdAsync(reserva.ApartamentoId, ct);
            if (apt == null) throw new InvalidOperationException("Apartamento não encontrado.");

            apt.Reservar();
            reserva.Iniciar();

            await _reservaRepo.AddAsync(reserva, ct);
            await _apartRepo.UpdateAsync(apt, ct);

            _logger.LogInformation("Reserva {ReservaId} criada para apartamento {ApartamentoId}", reserva.Id, reserva.ApartamentoId);
            return reserva;
        }

        public async Task ConfirmAsync(Guid id, CancellationToken ct = default)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id, ct);
            if (reserva == null) throw new InvalidOperationException("Reserva não encontrada.");

            var apt = await _apartRepo.GetByIdAsync(reserva.ApartamentoId, ct);

            reserva.Confirmar();
            apt?.Vender();

            var venda = new Venda
            {
                Id = Guid.NewGuid(),
                ClienteId = reserva.ClienteId,
                ApartamentoId = reserva.ApartamentoId,
                DataVenda = DateTime.UtcNow,
                ValorPago = apt?.Valor ?? 0m
            };

            var provider = _context.Database.ProviderName;
            if (provider != "Microsoft.EntityFrameworkCore.InMemory")
            {
                using var trx = await _context.Database.BeginTransactionAsync(ct);
                try
                {
                    await _vendaRepo.AddAsync(venda, ct);
                    if (apt != null) await _apartRepo.UpdateAsync(apt, ct);
                    await _reservaRepo.UpdateAsync(reserva, ct);
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
                if (apt != null) await _apartRepo.UpdateAsync(apt, ct);
                await _reservaRepo.UpdateAsync(reserva, ct);
            }

            _logger.LogInformation("Reserva {ReservaId} confirmada — Venda {VendaId} gerada", id, venda.Id);
        }

        public async Task CancelAsync(Guid id, CancellationToken ct = default)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id, ct);
            if (reserva == null) throw new InvalidOperationException("Reserva não encontrada.");

            var apt = await _apartRepo.GetByIdAsync(reserva.ApartamentoId, ct);

            reserva.Cancelar();
            apt?.Liberar();

            await _reservaRepo.UpdateAsync(reserva, ct);
            if (apt != null) await _apartRepo.UpdateAsync(apt, ct);

            _logger.LogInformation("Reserva {ReservaId} cancelada", id);
        }

        public Task UpdateAsync(Reserva reserva, CancellationToken ct = default)
            => _reservaRepo.UpdateAsync(reserva, ct);

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
            => _reservaRepo.DeleteAsync(id, ct);
    }
}
