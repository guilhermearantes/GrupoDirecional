using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByUsernameAsync(string username, CancellationToken ct = default);
    }
}
