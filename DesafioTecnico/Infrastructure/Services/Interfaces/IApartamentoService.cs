using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Services.Interfaces
{
    public interface IApartamentoService
    {
        Task<IEnumerable<Apartamento>> GetAllAsync();
        Task<Apartamento?> GetByIdAsync(Guid id);
        Task<Apartamento> CreateAsync(Apartamento apt);
        Task UpdateAsync(Apartamento apt);
        Task DeleteAsync(Guid id);
    }
}
