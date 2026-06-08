using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using DesafioTecnico.Application.Services.Interfaces;
using DesafioTecnico.Api.DTOs;
using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Api.Controllers
{
    /// <summary>
    /// Gerencia operações relacionadas às vendas de apartamentos.
    /// </summary>
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

        /// <summary>Lista todas as vendas com paginação.</summary>
        /// <param name="page">Número da página (padrão: 1).</param>
        /// <param name="pageSize">Itens por página (padrão: 20).</param>
        /// <param name="ct">Token de cancelamento da requisição.</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<VendaReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<VendaReadDto>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;
            var (items, total) = await _vendaService.GetPagedAsync(page, pageSize, ct);
            return Ok(new PagedResult<VendaReadDto>(_mapper.Map<IEnumerable<VendaReadDto>>(items), page, pageSize, total));
        }

        /// <summary>Retorna uma venda pelo identificador único.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VendaReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VendaReadDto>> Get(Guid id, CancellationToken ct = default)
        {
            var item = await _vendaService.GetByIdAsync(id, ct);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<VendaReadDto>(item));
        }

        /// <summary>Registra uma venda direta, sem reserva prévia.</summary>
        /// <remarks>Altera o status do apartamento para <c>Vendido</c>. Retorna 400 se o apartamento não estiver com status <c>Disponivel</c>.</remarks>
        [HttpPost]
        [ProducesResponseType(typeof(VendaReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post([FromBody] VendaCreateDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = _mapper.Map<Venda>(dto);
            var result = await _vendaService.CreateAsync(entity, ct);
            if (result.IsFailure) return BadRequest(new { error = result.Error });
            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, _mapper.Map<VendaReadDto>(result.Value));
        }

        /// <summary>Atualiza o valor pago de uma venda existente.</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put(Guid id, [FromBody] VendaUpdateDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _vendaService.GetByIdAsync(id, ct);
            if (existing == null) return NotFound();
            var toUpdate = _mapper.Map(dto, existing);
            await _vendaService.UpdateAsync(toUpdate, ct);
            return NoContent();
        }

        /// <summary>Remove uma venda pelo identificador único.</summary>
        /// <remarks>Em produção, prefira marcar a venda como estornada em vez de removê-la — vendas são registros contábeis. Disponível aqui por requisito do desafio.</remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            var existing = await _vendaService.GetByIdAsync(id, ct);
            if (existing == null) return NotFound();
            var result = await _vendaService.DeleteAsync(id, ct);
            return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
        }
    }
}
