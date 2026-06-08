using Moq;
using AutoMapper;
using DesafioTecnico.Api.Controllers;
using DesafioTecnico.Application.Services.Interfaces;
using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Results;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Controllers
{
    public class ReservasControllerTests
    {
        private readonly Mock<IReservaService> _serviceMock;
        private readonly IMapper _mapper;

        public ReservasControllerTests()
        {
            _serviceMock = new Mock<IReservaService>();
            _mapper = Fixtures.MapperFactory.Create();
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequest_QuandoModeloInvalido()
        {
            var controller = new ReservasController(_serviceMock.Object, _mapper);
            controller.ModelState.AddModelError("ClienteId", "Required");

            var result = await controller.Post(new DesafioTecnico.Api.DTOs.ReservaCreateDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Criar_DeveRetornarCreated_QuandoDadosValidos()
        {
            var reserva = new Reserva { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid(), DataReserva = DateTime.UtcNow, Status = DesafioTecnico.Domain.Enums.StatusReserva.Pendente };
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Reserva>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok(reserva));

            var controller = new ReservasController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.ReservaCreateDto { ClienteId = reserva.ClienteId, ApartamentoId = reserva.ApartamentoId };

            var res = await controller.Post(dto) as CreatedAtActionResult;

            Assert.NotNull(res);
            Assert.Equal("Get", res.ActionName);
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequest_QuandoServicoRetornaFalha()
        {
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Reserva>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Fail<Reserva>("Apartamento não disponível."));

            var controller = new ReservasController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.ReservaCreateDto { ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid() };

            var res = await controller.Post(dto) as BadRequestObjectResult;

            Assert.NotNull(res);
            Assert.Contains("Apartamento não disponível.", res.Value!.ToString());
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Reserva?)null);
            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Get(Guid.NewGuid());
            Assert.IsType<NotFoundResult>(res.Result);
        }

        [Fact]
        public async Task Confirmar_DeveRetornarBadRequest_QuandoServicoRetornaFalha()
        {
            _serviceMock.Setup(s => s.ConfirmAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Fail<Guid>("cannot confirm"));
            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Confirm(Guid.NewGuid()) as BadRequestObjectResult;

            Assert.NotNull(res);
            Assert.Contains("cannot confirm", res.Value!.ToString());
        }

        [Fact]
        public async Task Confirmar_DeveRetornarCreated_QuandoBemSucedido()
        {
            var vendaId = Guid.NewGuid();
            _serviceMock.Setup(s => s.ConfirmAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok(vendaId));
            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Confirm(Guid.NewGuid()) as CreatedAtActionResult;

            Assert.NotNull(res);
            Assert.Equal("Get", res!.ActionName);
        }

        [Fact]
        public async Task Confirmar_DeveRetornarNotFound_QuandoNaoEncontrada()
        {
            _serviceMock.Setup(s => s.ConfirmAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound<Guid>("Reserva não encontrada."));
            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Confirm(Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(res);
        }

        [Fact]
        public async Task Cancelar_DeveRetornarBadRequest_QuandoServicoRetornaFalha()
        {
            _serviceMock.Setup(s => s.CancelAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Fail("cannot cancel"));
            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Cancel(Guid.NewGuid()) as BadRequestObjectResult;

            Assert.NotNull(res);
            Assert.Contains("cannot cancel", res.Value!.ToString());
        }

        [Fact]
        public async Task Cancelar_DeveRetornarNoContent_QuandoBemSucedido()
        {
            _serviceMock.Setup(s => s.CancelAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok());
            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Cancel(Guid.NewGuid());
            Assert.IsType<NoContentResult>(res);
        }

        [Fact]
        public async Task Cancelar_DeveRetornarNotFound_QuandoNaoEncontrada()
        {
            _serviceMock.Setup(s => s.CancelAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound("Reserva não encontrada."));
            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Cancel(Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(res);
        }

        [Fact]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Reserva?)null);
            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Delete(Guid.NewGuid());
            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public async Task Excluir_DeveRetornarNoContent_QuandoExiste()
        {
            var reserva = new Reserva { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid() };
            _serviceMock.Setup(s => s.GetByIdAsync(reserva.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reserva);
            _serviceMock.Setup(s => s.DeleteAsync(reserva.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok()).Verifiable();

            var controller = new ReservasController(_serviceMock.Object, _mapper);
            var res = await controller.Delete(reserva.Id);

            Assert.IsType<NoContentResult>(res);
            _serviceMock.Verify();
        }
    }
}
