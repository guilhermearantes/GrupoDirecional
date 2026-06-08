namespace DesafioTecnico.Application.Services.Interfaces
{
    /// <summary>
    /// Define operações de autenticação de usuários.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Autentica um usuário e retorna um token JWT.
        /// </summary>
        /// <returns>
        /// Tupla com o token e o tempo de expiração em segundos,
        /// ou <c>null</c> quando as credenciais forem inválidas.
        /// </returns>
        Task<(string Token, int ExpiresInSeconds)?> AuthenticateAsync(string username, string password, CancellationToken ct = default);
    }
}
