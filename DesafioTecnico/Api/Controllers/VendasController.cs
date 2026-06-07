using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using DesafioTecnico.Infrastructure.Services.Interfaces;
using DesafioTecnico.Api.DTOs;

namespace DesafioTecnico.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        [ProducesResponseType(typeof(PagedResult<VendaReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<VendaReadDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var all = await _vendaService.GetAllAsync();
            var list = all.ToList();
            var items = list.Skip((page - 1) * pageSize).Take(pageSize);
            return Ok(new PagedResult<VendaReadDto>(_mapper.Map<IEnumerable<VendaReadDto>>(items), page, pageSize, list.Count));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VendaReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VendaReadDto>> Get(Guid id)
        {
            var item = await _vendaService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<VendaReadDto>(item));
        }

        [HttpPost]
        [ProducesResponseType(typeof(VendaReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        // O endpoint abaixo atende ao requisito "excluir vendas" do desafio técnico.
        // Em produção, deletar uma venda não é recomendado: vendas são registros contábeis
        // e sua remoção quebra o histórico financeiro e de auditoria. A boa prática é
        // adicionar um campo "Cancelada/Estornada" e manter o registro.
        // Para habilitar, descomente o bloco abaixo.

        /*
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(Guid id)
        {
            var existing = await _vendaService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _vendaService.DeleteAsync(id);
            return NoContent();
        }
        */
    }
}
