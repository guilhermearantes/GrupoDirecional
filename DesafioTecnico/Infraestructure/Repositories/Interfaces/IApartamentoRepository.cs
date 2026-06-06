using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infraestructure.Repositories.Interfaces
{
    public interface IApartamentoRepository
    {
        Task<IEnumerable<Apartamento>> GetAllAsync();
        Task<Apartamento?> GetByIdAsync(Guid id);
        Task AddAsync(Apartamento apt);
        Task UpdateAsync(Apartamento apt);
        Task DeleteAsync(Guid id);
        Task<bool> IsAvailableAsync(Guid apartamentoId);
    }
}
