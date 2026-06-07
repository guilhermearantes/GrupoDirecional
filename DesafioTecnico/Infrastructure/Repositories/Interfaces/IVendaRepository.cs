using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Repositories.Interfaces
{
    public interface IVendaRepository
    {
        Task<IEnumerable<Venda>> GetAllAsync();
        Task<Venda?> GetByIdAsync(Guid id);
        Task AddAsync(Venda venda);
        Task UpdateAsync(Venda venda);
        Task DeleteAsync(Guid id);
    }
}
