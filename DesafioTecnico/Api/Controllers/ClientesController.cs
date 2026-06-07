using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DesafioTecnico.Infraestructure.Services.Interfaces;
using DesafioTecnico.Api.DTOs;

namespace DesafioTecnico.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
        public async Task<ActionResult<IEnumerable<ClienteReadDto>>> Get()
        {
            var items = await _service.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<ClienteReadDto>>(items));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteReadDto>> Get(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<ClienteReadDto>(item));
        }

        [HttpPost]
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
        public async Task<ActionResult> Delete(Guid id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
