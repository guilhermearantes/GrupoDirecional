using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DesafioTecnico.Api.DTOs;

namespace DesafioTecnico.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly Infraestructure.Services.Interfaces.IAuthService _authService;

        public AuthController(Infraestructure.Services.Interfaces.IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.AuthenticateAsync(request.Username, request.Password);
            if (result == null) return Unauthorized();

            return Ok(new LoginResponse { Token = result.Value.Token, ExpiresIn = result.Value.ExpiresInSeconds });
        }
    }
}
