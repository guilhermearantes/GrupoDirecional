using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using DesafioTecnico.Infrastructure.Services.Interfaces;
using DesafioTecnico.Api.DTOs;
using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Api.Controllers
{
    /// <summary>
    /// Gerencia o ciclo de vida de reservas: criação, confirmação, cancelamento e exclusão.
    /// </summary>
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

        /// <summary>Lista todas as reservas com paginação.</summary>
        /// <param name="page">Número da página (padrão: 1).</param>
        /// <param name="pageSize">Itens por página (padrão: 20).</param>
        /// <param name="ct">Token de cancelamento da requisição.</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ReservaReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ReservaReadDto>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;
            var (items, total) = await _service.GetPagedAsync(page, pageSize, ct);
            return Ok(new PagedResult<ReservaReadDto>(_mapper.Map<IEnumerable<ReservaReadDto>>(items), page, pageSize, total));
        }

        /// <summary>Retorna uma reserva pelo identificador único.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReservaReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReservaReadDto>> Get(Guid id, CancellationToken ct = default)
        {
            var item = await _service.GetByIdAsync(id, ct);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<ReservaReadDto>(item));
        }

        /// <summary>Cria uma nova reserva para um apartamento disponível.</summary>
        /// <remarks>Altera o status do apartamento para <c>Reservado</c>. Retorna 400 se o apartamento não estiver com status <c>Disponivel</c>.</remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ReservaReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post([FromBody] ReservaCreateDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = _mapper.Map<Reserva>(dto);
            var result = await _service.CreateAsync(entity, ct);
            if (result.IsFailure) return BadRequest(new { error = result.Error });
            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, _mapper.Map<ReservaReadDto>(result.Value));
        }

        /// <summary>
        /// Confirma uma reserva pendente, gera a venda correspondente e marca o apartamento como Vendido.
        /// </summary>
        [HttpPost("{id}/confirm")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Confirm(Guid id, CancellationToken ct = default)
        {
            var result = await _service.ConfirmAsync(id, ct);
            return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
        }

        /// <summary>
        /// Cancela uma reserva pendente e devolve o apartamento ao status Disponível.
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Cancel(Guid id, CancellationToken ct = default)
        {
            var result = await _service.CancelAsync(id, ct);
            return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
        }

        /// <summary>Remove uma reserva pelo identificador único.</summary>
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
