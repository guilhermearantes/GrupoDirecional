namespace DesafioTecnico.Infrastructure.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(string Token, int ExpiresInSeconds)?> AuthenticateAsync(string username, string password, CancellationToken ct = default);
    }
}
