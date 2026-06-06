using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using DesafioTecnico.Infraestructure.Repositories.Interfaces;
using DesafioTecnico.Api.DTOs;

namespace DesafioTecnico.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApartamentosController : ControllerBase
    {
        private readonly IApartamentoRepository _repo;
        private readonly IMapper _mapper;

        public ApartamentosController(IApartamentoRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApartamentoReadDto>>> Get()
        {
            var items = await _repo.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<ApartamentoReadDto>>(items));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApartamentoReadDto>> Get(Guid id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<ApartamentoReadDto>(item));
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ApartamentoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = _mapper.Map<DesafioTecnico.Domain.Entities.Apartamento>(dto);
            entity.Id = Guid.NewGuid();
            entity.Status = DesafioTecnico.Domain.Enums.StatusApartamento.Disponivel;
            await _repo.AddAsync(entity);
            return CreatedAtAction(nameof(Get), new { id = entity.Id }, _mapper.Map<ApartamentoReadDto>(entity));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] ApartamentoUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var toUpdate = _mapper.Map(dto, existing);
            toUpdate.Id = id;
            await _repo.UpdateAsync(toUpdate);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
