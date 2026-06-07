using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DesafioTecnico.Infrastructure.Services.Interfaces;
using DesafioTecnico.Api.DTOs;

namespace DesafioTecnico.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _service;
        private readonly IMapper _mapper;

        public ClientesController(IClienteService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ClienteReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ClienteReadDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var all = await _service.GetAllAsync();
            var list = all.ToList();
            var items = list.Skip((page - 1) * pageSize).Take(pageSize);
            return Ok(new PagedResult<ClienteReadDto>(_mapper.Map<IEnumerable<ClienteReadDto>>(items), page, pageSize, list.Count));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClienteReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClienteReadDto>> Get(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<ClienteReadDto>(item));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ClienteReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post([FromBody] ClienteCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = _mapper.Map<Domain.Entities.Cliente>(dto);
            try
            {
                await _service.CreateAsync(entity);
            }
            catch (DbUpdateException)
            {
                return Conflict(new { error = "Email ou CPF já cadastrado." });
            }
            return CreatedAtAction(nameof(Get), new { id = entity.Id }, _mapper.Map<ClienteReadDto>(entity));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Put(Guid id, [FromBody] ClienteUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var toUpdate = _mapper.Map(dto, existing);
            toUpdate.Id = id;
            try
            {
                await _service.UpdateAsync(toUpdate);
            }
            catch (DbUpdateException)
            {
                return Conflict(new { error = "Email ou CPF já cadastrado." });
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(Guid id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
