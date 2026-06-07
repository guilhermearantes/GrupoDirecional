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

        public Task<IEnumerable<Reserva>> GetAllAsync() => _reservaRepo.GetAllAsync();

        public Task<Reserva?> GetByIdAsync(Guid id) => _reservaRepo.GetByIdAsync(id);

        public async Task<Reserva> CreateAsync(Reserva reserva)
        {
            var apt = await _apartRepo.GetByIdAsync(reserva.ApartamentoId);
            if (apt == null) throw new InvalidOperationException("Apartamento não encontrado.");

            apt.Reservar();
            reserva.Iniciar();

            await _reservaRepo.AddAsync(reserva);
            await _apartRepo.UpdateAsync(apt);

            _logger.LogInformation("Reserva {ReservaId} criada para apartamento {ApartamentoId}", reserva.Id, reserva.ApartamentoId);
            return reserva;
        }

        public async Task ConfirmAsync(Guid id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id);
            if (reserva == null) throw new InvalidOperationException("Reserva não encontrada.");

            var apt = await _apartRepo.GetByIdAsync(reserva.ApartamentoId);

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
                using var trx = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _vendaRepo.AddAsync(venda);
                    if (apt != null) await _apartRepo.UpdateAsync(apt);
                    await _reservaRepo.UpdateAsync(reserva);
                    await trx.CommitAsync();
                }
                catch
                {
                    await trx.RollbackAsync();
                    throw;
                }
            }
            else
            {
                await _vendaRepo.AddAsync(venda);
                if (apt != null) await _apartRepo.UpdateAsync(apt);
                await _reservaRepo.UpdateAsync(reserva);
            }

            _logger.LogInformation("Reserva {ReservaId} confirmada — Venda {VendaId} gerada", id, venda.Id);
        }

        public async Task CancelAsync(Guid id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id);
            if (reserva == null) throw new InvalidOperationException("Reserva não encontrada.");

            var apt = await _apartRepo.GetByIdAsync(reserva.ApartamentoId);

            reserva.Cancelar();
            apt?.Liberar();

            await _reservaRepo.UpdateAsync(reserva);
            if (apt != null) await _apartRepo.UpdateAsync(apt);

            _logger.LogInformation("Reserva {ReservaId} cancelada", id);
        }

        public Task UpdateAsync(Reserva reserva) => _reservaRepo.UpdateAsync(reserva);

        public Task DeleteAsync(Guid id) => _reservaRepo.DeleteAsync(id);
    }
}
