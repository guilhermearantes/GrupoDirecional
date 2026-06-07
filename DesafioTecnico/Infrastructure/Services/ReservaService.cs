namespace DesafioTecnico.Infrastructure.Services
{
    using DesafioTecnico.Infrastructure.Repositories.Interfaces;
    using DesafioTecnico.Domain.Entities;
    using DesafioTecnico.Infrastructure.Data;
    using DesafioTecnico.Infrastructure.Services.Interfaces;

    public class ReservaService : IReservaService
    {
        private readonly AppDbContext _context;
        private readonly IReservaRepository _reservaRepo;
        private readonly IApartamentoRepository _apartRepo;
        private readonly IVendaRepository _vendaRepo;

        public ReservaService(AppDbContext context, IReservaRepository reservaRepo, IApartamentoRepository apartRepo, IVendaRepository vendaRepo)
        {
            _context = context;
            _reservaRepo = reservaRepo;
            _apartRepo = apartRepo;
            _vendaRepo = vendaRepo;
        }

        public Task<IEnumerable<Reserva>> GetAllAsync() => _reservaRepo.GetAllAsync();

        public Task<Reserva?> GetByIdAsync(Guid id) => _reservaRepo.GetByIdAsync(id);

        public async Task<Reserva> CreateAsync(Reserva reserva)
        {
            var available = await _apartRepo.IsAvailableAsync(reserva.ApartamentoId);
            if (!available) throw new InvalidOperationException("Apartamento não disponível para reserva");

            reserva.Id = Guid.NewGuid();
            reserva.DataReserva = DateTime.UtcNow;
            reserva.Status = Domain.Enums.StatusReserva.Pendente;

            await _reservaRepo.AddAsync(reserva);

            var apt = await _apartRepo.GetByIdAsync(reserva.ApartamentoId);
            if (apt != null)
            {
                apt.Status = Domain.Enums.StatusApartamento.Reservado;
                await _apartRepo.UpdateAsync(apt);
            }

            return reserva;
        }

        public async Task ConfirmAsync(Guid id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id);
            if (reserva == null) throw new InvalidOperationException("Reserva não encontrada");
            if (reserva.Status != Domain.Enums.StatusReserva.Pendente) throw new InvalidOperationException("Reserva não está em estado pendente");

            var apt = await _apartRepo.GetByIdAsync(reserva.ApartamentoId);

            var venda = new Venda
            {
                Id = Guid.NewGuid(),
                ClienteId = reserva.ClienteId,
                ApartamentoId = reserva.ApartamentoId,
                DataVenda = DateTime.UtcNow,
                ValorPago = apt?.Valor ?? 0m
            };

            // Wrap the entire confirmation (venda + status updates) in a single transaction
            var provider = _context.Database.ProviderName;
            if (provider != "Microsoft.EntityFrameworkCore.InMemory")
            {
                using var trx = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _vendaRepo.AddAsync(venda);
                    if (apt != null) { apt.Status = Domain.Enums.StatusApartamento.Vendido; await _apartRepo.UpdateAsync(apt); }
                    reserva.Status = Domain.Enums.StatusReserva.Confirmada;
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
                if (apt != null) { apt.Status = Domain.Enums.StatusApartamento.Vendido; await _apartRepo.UpdateAsync(apt); }
                reserva.Status = Domain.Enums.StatusReserva.Confirmada;
                await _reservaRepo.UpdateAsync(reserva);
            }
        }

        public async Task CancelAsync(Guid id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id);
            if (reserva == null) throw new InvalidOperationException("Reserva não encontrada");
            if (reserva.Status != Domain.Enums.StatusReserva.Pendente) throw new InvalidOperationException("Somente reservas pendentes podem ser canceladas");

            reserva.Status = Domain.Enums.StatusReserva.Cancelada;
            await _reservaRepo.UpdateAsync(reserva);

            var apt = await _apartRepo.GetByIdAsync(reserva.ApartamentoId);
            if (apt != null)
            {
                apt.Status = Domain.Enums.StatusApartamento.Disponivel;
                await _apartRepo.UpdateAsync(apt);
            }
        }

        public Task UpdateAsync(Reserva reserva) => _reservaRepo.UpdateAsync(reserva);

        public Task DeleteAsync(Guid id) => _reservaRepo.DeleteAsync(id);
    }
}
