using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Infrastructure.Data;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;
using DesafioTecnico.Infrastructure.Services.Interfaces;

namespace DesafioTecnico.Infrastructure.Services
{
    public class VendaService : IVendaService
    {
        private readonly AppDbContext _context;
        private readonly IVendaRepository _vendaRepo;
        private readonly IApartamentoRepository _apartRepo;

        public VendaService(AppDbContext context, IVendaRepository vendaRepo, IApartamentoRepository apartRepo)
        {
            _context = context;
            _vendaRepo = vendaRepo;
            _apartRepo = apartRepo;
        }

        public Task<IEnumerable<Venda>> GetAllAsync() => _vendaRepo.GetAllAsync();

        public Task<Venda?> GetByIdAsync(Guid id) => _vendaRepo.GetByIdAsync(id);

        public async Task<Venda> CreateAsync(Venda venda)
        {
            // Allow sale if apartment exists and is not already sold. Reservado is allowed (confirmation flow).
            var aptCheck = await _apartRepo.GetByIdAsync(venda.ApartamentoId);
            if (aptCheck == null) throw new InvalidOperationException("Apartamento não encontrado");
            if (aptCheck.Status == Domain.Enums.StatusApartamento.Vendido) throw new InvalidOperationException("Apartamento não disponível");

            venda.Id = Guid.NewGuid();
            venda.DataVenda = DateTime.UtcNow;

            // Some providers (InMemory) do not support transactions; only use explicit transaction when supported
            var provider = _context.Database.ProviderName;
            if (provider != "Microsoft.EntityFrameworkCore.InMemory")
            {
                using var trx = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _vendaRepo.AddAsync(venda);

                    var apt = await _apartRepo.GetByIdAsync(venda.ApartamentoId);
                    if (apt != null)
                    {
                        apt.Status = Domain.Enums.StatusApartamento.Vendido;
                        await _apartRepo.UpdateAsync(apt);
                    }

                    await trx.CommitAsync();
                    return venda;
                }
                catch
                {
                    await trx.RollbackAsync();
                    throw;
                }
            }
            else
            {
                // InMemory: perform sequential operations
                await _vendaRepo.AddAsync(venda);
                var apt = await _apartRepo.GetByIdAsync(venda.ApartamentoId);
                if (apt != null)
                {
                    apt.Status = Domain.Enums.StatusApartamento.Vendido;
                    await _apartRepo.UpdateAsync(apt);
                }
                return venda;
            }
        }

        public Task UpdateAsync(Venda venda) => _vendaRepo.UpdateAsync(venda);

        public Task DeleteAsync(Guid id) => _vendaRepo.DeleteAsync(id);
    }
}
