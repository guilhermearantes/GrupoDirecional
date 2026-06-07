using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using DesafioTecnico.Infrastructure.Services.Interfaces;
using DesafioTecnico.Api.DTOs;

namespace DesafioTecnico.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        [ProducesResponseType(typeof(PagedResult<ReservaReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ReservaReadDto>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var all = await _service.GetAllAsync(ct);
            var list = all.ToList();
            var items = list.Skip((page - 1) * pageSize).Take(pageSize);
            return Ok(new PagedResult<ReservaReadDto>(_mapper.Map<IEnumerable<ReservaReadDto>>(items), page, pageSize, list.Count));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReservaReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReservaReadDto>> Get(Guid id, CancellationToken ct = default)
        {
            var item = await _service.GetByIdAsync(id, ct);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<ReservaReadDto>(item));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReservaReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post([FromBody] ReservaCreateDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = _mapper.Map<DesafioTecnico.Domain.Entities.Reserva>(dto);
            await _service.CreateAsync(entity, ct);
            return CreatedAtAction(nameof(Get), new { id = entity.Id }, _mapper.Map<ReservaReadDto>(entity));
        }

        [HttpPost("{id}/confirm")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Confirm(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _service.ConfirmAsync(id, ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Cancel(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _service.CancelAsync(id, ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            var existing = await _service.GetByIdAsync(id, ct);
            if (existing == null) return NotFound();
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
