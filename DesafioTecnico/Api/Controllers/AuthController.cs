using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DesafioTecnico.Api.DTOs;
using DesafioTecnico.Infraestructure.Services;

namespace DesafioTecnico.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var token = await _authService.AuthenticateAsync(request.Username, request.Password);
            if (token == null) return Unauthorized();

            return Ok(new LoginResponse { Token = token, ExpiresIn = "" });
        }
    }
}
