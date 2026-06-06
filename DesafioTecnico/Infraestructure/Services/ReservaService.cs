namespace DesafioTecnico.Infraestructure.Services
{
    using DesafioTecnico.Infraestructure.Repositories.Interfaces;
    using DesafioTecnico.Domain.Entities;
    using DesafioTecnico.Infraestructure.Services.Interfaces;

    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepo;
        private readonly IApartamentoRepository _apartRepo;
        private readonly IVendaService _vendaService;

        public ReservaService(IReservaRepository reservaRepo, IApartamentoRepository apartRepo, IVendaService vendaService)
        {
            _reservaRepo = reservaRepo;
            _apartRepo = apartRepo;
            _vendaService = vendaService;
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

            // Criar venda baseada na reserva
            var venda = new Venda
            {
                ClienteId = reserva.ClienteId,
                ApartamentoId = reserva.ApartamentoId,
                ValorPago = (await _apartRepo.GetByIdAsync(reserva.ApartamentoId))?.Valor ?? 0m
            };

            await _vendaService.CreateAsync(venda);

            reserva.Status = Domain.Enums.StatusReserva.Confirmada;
            await _reservaRepo.UpdateAsync(reserva);
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
