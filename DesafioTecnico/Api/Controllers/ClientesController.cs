using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DesafioTecnico.Application.Services.Interfaces;
using DesafioTecnico.Api.DTOs;

namespace DesafioTecnico.Api.Controllers
{
    /// <summary>
    /// Gerencia operações relacionadas aos clientes.
    /// </summary>
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

        /// <summary>Lista todos os clientes com paginação.</summary>
        /// <param name="page">Número da página (padrão: 1).</param>
        /// <param name="pageSize">Itens por página (padrão: 20).</param>
        /// <param name="ct">Token de cancelamento da requisição.</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ClienteReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ClienteReadDto>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;
            var (items, total) = await _service.GetPagedAsync(page, pageSize, ct);
            return Ok(new PagedResult<ClienteReadDto>(_mapper.Map<IEnumerable<ClienteReadDto>>(items), page, pageSize, total));
        }

        /// <summary>Retorna um cliente pelo identificador único.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClienteReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClienteReadDto>> Get(Guid id, CancellationToken ct = default)
        {
            var item = await _service.GetByIdAsync(id, ct);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<ClienteReadDto>(item));
        }

        /// <summary>Cadastra um novo cliente.</summary>
        /// <remarks>E-mail e CPF são únicos — retorna 409 se já existirem no sistema.</remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ClienteReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post([FromBody] ClienteCreateDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = _mapper.Map<Domain.Entities.Cliente>(dto);
            Domain.Entities.Cliente created;
            try
            {
                created = await _service.CreateAsync(entity, ct);
            }
            catch (DbUpdateException)
            {
                return Conflict(new ErrorResponse("Email ou CPF já cadastrado."));
            }
            return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<ClienteReadDto>(created));
        }

        /// <summary>Atualiza os dados de um cliente existente.</summary>
        /// <remarks>Retorna 409 se o novo e-mail ou CPF já pertencerem a outro cliente.</remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Put(Guid id, [FromBody] ClienteUpdateDto dto, CancellationToken ct = default)
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
                return Conflict(new ErrorResponse("Email ou CPF já cadastrado."));
            }
            return NoContent();
        }

        /// <summary>Remove um cliente pelo identificador único.</summary>
        /// <remarks>Retorna 409 se o cliente possuir reservas ou vendas associadas.</remarks>
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
                return Conflict(new ErrorResponse("Não é possível excluir um cliente com reservas ou vendas associadas."));
            }
            return NoContent();
        }
    }
}
