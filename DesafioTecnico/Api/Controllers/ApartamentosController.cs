using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DesafioTecnico.Application.Services.Interfaces;
using DesafioTecnico.Api.DTOs;
using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Enums;

namespace DesafioTecnico.Api.Controllers
{
    /// <summary>
    /// Gerencia operações relacionadas aos apartamentos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class ApartamentosController : ControllerBase
    {
        private readonly IApartamentoService _service;
        private readonly IMapper _mapper;

        public ApartamentosController(IApartamentoService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Lista apartamentos com suporte a paginação e filtro por status.</summary>
        /// <param name="status">Filtro opcional: <c>Disponivel</c>, <c>Reservado</c> ou <c>Vendido</c>. Omitir retorna todos.</param>
        /// <param name="page">Número da página (padrão: 1).</param>
        /// <param name="pageSize">Itens por página (padrão: 20).</param>
        /// <param name="ct">Token de cancelamento da requisição.</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ApartamentoReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ApartamentoReadDto>>> Get(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;
            StatusApartamento? statusEnum = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<StatusApartamento>(status, ignoreCase: true, out var parsed))
                statusEnum = parsed;
            var (items, total) = await _service.GetPagedAsync(page, pageSize, statusEnum, ct);
            return Ok(new PagedResult<ApartamentoReadDto>(_mapper.Map<IEnumerable<ApartamentoReadDto>>(items), page, pageSize, total));
        }

        /// <summary>Retorna um apartamento pelo identificador único.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApartamentoReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApartamentoReadDto>> Get(Guid id, CancellationToken ct = default)
        {
            var item = await _service.GetByIdAsync(id, ct);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<ApartamentoReadDto>(item));
        }

        /// <summary>Cadastra um novo apartamento.</summary>
        /// <remarks>Código é único — retorna 409 se já existir no sistema.</remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApartamentoReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post([FromBody] ApartamentoCreateDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = _mapper.Map<Apartamento>(dto);
            Apartamento created;
            try
            {
                created = await _service.CreateAsync(entity, ct);
            }
            catch (DbUpdateException)
            {
                return Conflict(new ErrorResponse("Código de apartamento já cadastrado."));
            }
            return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<ApartamentoReadDto>(created));
        }

        /// <summary>Atualiza os dados de um apartamento existente.</summary>
        /// <remarks>Retorna 409 se o novo código já pertencer a outro apartamento.</remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Put(Guid id, [FromBody] ApartamentoUpdateDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _service.GetByIdAsync(id, ct);
            if (existing == null) return NotFound();
            var toUpdate = _mapper.Map(dto, existing);
            try
            {
                await _service.UpdateAsync(toUpdate, ct);
            }
            catch (DbUpdateException)
            {
                return Conflict(new ErrorResponse("Código de apartamento já cadastrado."));
            }
            return NoContent();
        }

        /// <summary>Remove um apartamento pelo identificador único.</summary>
        /// <remarks>Retorna 409 se o apartamento possuir reservas ou vendas associadas.</remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            var existing = await _service.GetByIdAsync(id, ct);
            if (existing == null) return NotFound();
            try
            {
                await _service.DeleteAsync(id, ct);
            }
            catch (DbUpdateException)
            {
                return Conflict(new ErrorResponse("Não é possível excluir um apartamento com reservas ou vendas associadas."));
            }
            return NoContent();
        }
    }
}
