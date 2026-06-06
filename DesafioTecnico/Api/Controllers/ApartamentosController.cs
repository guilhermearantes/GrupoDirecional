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
    public class ApartamentosController : ControllerBase
    {
        private readonly IApartamentoService _service;
        private readonly IMapper _mapper;

        public ApartamentosController(IApartamentoService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApartamentoReadDto>>> Get()
        {
            var items = await _service.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<ApartamentoReadDto>>(items));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApartamentoReadDto>> Get(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<ApartamentoReadDto>(item));
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ApartamentoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = _mapper.Map<DesafioTecnico.Domain.Entities.Apartamento>(dto);
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<ApartamentoReadDto>(created));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] ApartamentoUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var toUpdate = _mapper.Map(dto, existing);
            toUpdate.Id = id;
            await _service.UpdateAsync(toUpdate);
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
