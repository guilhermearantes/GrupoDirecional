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
    public class VendasController : ControllerBase
    {
        private readonly IVendaService _vendaService;
        private readonly IMapper _mapper;

        public VendasController(IVendaService vendaService, IMapper mapper)
        {
            _vendaService = vendaService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VendaReadDto>>> Get()
        {
            var items = await _vendaService.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<VendaReadDto>>(items));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VendaReadDto>> Get(Guid id)
        {
            var item = await _vendaService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<VendaReadDto>(item));
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] VendaCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = _mapper.Map<DesafioTecnico.Domain.Entities.Venda>(dto);
            try
            {
                var created = await _vendaService.CreateAsync(entity);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<VendaReadDto>(created));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] VendaUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _vendaService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var toUpdate = _mapper.Map(dto, existing);
            toUpdate.Id = id;
            await _vendaService.UpdateAsync(toUpdate);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var existing = await _vendaService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _vendaService.DeleteAsync(id);
            return NoContent();
        }
    }
}
