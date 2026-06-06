using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infraestructure.Services.Interfaces
{
    public interface IClienteService
    {
        Task<IEnumerable<Cliente>> GetAllAsync();
        Task<Cliente?> GetByIdAsync(Guid id);
        Task CreateAsync(Cliente cliente);
        Task UpdateAsync(Cliente cliente);
        Task DeleteAsync(Guid id);
    }
}
