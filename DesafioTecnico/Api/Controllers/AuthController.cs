using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using DesafioTecnico.Api.DTOs;
using DesafioTecnico.Infrastructure.Services.Interfaces;

namespace DesafioTecnico.Api.Controllers
{
    /// <summary>
    /// Gerencia autenticação de usuários e emissão de tokens JWT.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Autentica um usuário e retorna um token JWT Bearer.</summary>
        /// <remarks>Rate limiting aplicado: máximo de 5 tentativas por minuto por IP.</remarks>
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.AuthenticateAsync(request.Username, request.Password, ct);
            if (result == null) return Unauthorized();

            return Ok(new LoginResponse { Token = result.Value.Token, ExpiresIn = result.Value.ExpiresInSeconds });
        }
    }
}
