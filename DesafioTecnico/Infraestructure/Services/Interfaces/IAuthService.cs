namespace DesafioTecnico.Infraestructure.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(string Token, int ExpiresInSeconds)?> AuthenticateAsync(string username, string password);
    }
}
