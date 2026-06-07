using System.ComponentModel.DataAnnotations;

namespace DesafioTecnico.Api.DTOs
{
    public class LoginRequest
    {
        /// <summary>Nome de usuário cadastrado no sistema.</summary>
        /// <example>admin</example>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>Senha do usuário.</summary>
        /// <example>admin123</example>
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        /// <summary>Token JWT Bearer para uso no header <c>Authorization: Bearer &lt;token&gt;</c>.</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>Tempo de expiração do token em segundos.</summary>
        /// <example>3600</example>
        public int ExpiresIn { get; set; }
    }
}
