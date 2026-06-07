using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Services.Interfaces
{
    public interface IVendaService
    {
        Task<IEnumerable<Venda>> GetAllAsync();
        Task<Venda?> GetByIdAsync(Guid id);
        Task<Venda> CreateAsync(Venda venda);
        Task UpdateAsync(Venda venda);
        Task DeleteAsync(Guid id);
    }
}
