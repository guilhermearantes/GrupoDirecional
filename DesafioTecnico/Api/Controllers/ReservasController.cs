using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using DesafioTecnico.Infraestructure.Services.Interfaces;
using DesafioTecnico.Api.DTOs;

namespace DesafioTecnico.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservasController : ControllerBase
    {
        private readonly IReservaService _service;
        private readonly IMapper _mapper;

        public ReservasController(IReservaService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservaReadDto>>> Get()
        {
            var items = await _service.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<ReservaReadDto>>(items));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReservaReadDto>> Get(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<ReservaReadDto>(item));
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ReservaCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = _mapper.Map<DesafioTecnico.Domain.Entities.Reserva>(dto);
            await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(Get), new { id = entity.Id }, _mapper.Map<ReservaReadDto>(entity));
        }

        [HttpPost("{id}/confirm")]
        public async Task<ActionResult> Confirm(Guid id)
        {
            try
            {
                await _service.ConfirmAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> Cancel(Guid id)
        {
            try
            {
                await _service.CancelAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
