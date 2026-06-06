using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infraestructure.Repositories.Interfaces
{
    public interface IClienteRepository
    {
        Task<IEnumerable<Cliente>> GetAllAsync();
        Task<Cliente?> GetByIdAsync(Guid id);
        Task AddAsync(Cliente cliente);
        Task UpdateAsync(Cliente cliente);
        Task DeleteAsync(Guid id);
    }
}
