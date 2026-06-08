using Moq;
using AutoMapper;
using DesafioTecnico.Api.Controllers;
using DesafioTecnico.Application.Services.Interfaces;
using DesafioTecnico.Domain.Entities;
using DesafioTecnico.Domain.Results;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Controllers
{
    public class VendasControllerTests
    {
        private readonly Mock<IVendaService> _serviceMock;
        private readonly IMapper _mapper;

        public VendasControllerTests()
        {
            _serviceMock = new Mock<IVendaService>();
            _mapper = Fixtures.MapperFactory.Create();
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequest_QuandoModeloInvalido()
        {
            var controller = new VendasController(_serviceMock.Object, _mapper);
            controller.ModelState.AddModelError("ClienteId", "Required");

            var dto = new DesafioTecnico.Api.DTOs.VendaCreateDto();
            var result = await controller.Post(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequest_QuandoServicoRetornaFalha()
        {
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Venda>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Fail<Venda>("nope"));
            var controller = new VendasController(_serviceMock.Object, _mapper);

            var dto = new DesafioTecnico.Api.DTOs.VendaCreateDto { ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid(), ValorPago = 100m };
            var res = await controller.Post(dto) as BadRequestObjectResult;

            Assert.NotNull(res);
            Assert.Contains("nope", res.Value!.ToString());
        }

        [Fact]
        public async Task Criar_DeveRetornarCreated_QuandoDadosValidos()
        {
            var venda = new Venda { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid(), ValorPago = 100m, DataVenda = DateTime.UtcNow };
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Venda>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok(venda));

            var controller = new VendasController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.VendaCreateDto { ClienteId = venda.ClienteId, ApartamentoId = venda.ApartamentoId, ValorPago = venda.ValorPago };

            var res = await controller.Post(dto) as CreatedAtActionResult;

            Assert.NotNull(res);
            Assert.Equal("Get", res.ActionName);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Venda?)null);
            var controller = new VendasController(_serviceMock.Object, _mapper);

            var dto = new DesafioTecnico.Api.DTOs.VendaUpdateDto { ValorPago = 200m };
            var res = await controller.Put(Guid.NewGuid(), dto);

            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarNoContent_QuandoExiste()
        {
            var venda = new Venda { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid(), ValorPago = 100m, DataVenda = DateTime.UtcNow };
            _serviceMock.Setup(s => s.GetByIdAsync(venda.Id, It.IsAny<CancellationToken>())).ReturnsAsync(venda);
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Venda>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var controller = new VendasController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.VendaUpdateDto { ValorPago = 250m };

            var res = await controller.Put(venda.Id, dto);

            Assert.IsType<NoContentResult>(res);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Venda>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarBadRequest_QuandoModeloInvalido()
        {
            var venda = new Venda { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid(), ValorPago = 100m, DataVenda = DateTime.UtcNow };
            var controller = new VendasController(_serviceMock.Object, _mapper);
            controller.ModelState.AddModelError("ValorPago", "Required");

            var dto = new DesafioTecnico.Api.DTOs.VendaUpdateDto();
            var res = await controller.Put(venda.Id, dto) as BadRequestObjectResult;

            Assert.NotNull(res);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Venda?)null);
            var controller = new VendasController(_serviceMock.Object, _mapper);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound("Venda não encontrada."));
            var controller = new VendasController(_serviceMock.Object, _mapper);

            var res = await controller.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundObjectResult>(res);
        }

        [Fact]
        public async Task Excluir_DeveRetornarNoContent_QuandoExiste()
        {
            var venda = new Venda { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid(), ValorPago = 100m, DataVenda = DateTime.UtcNow };
            _serviceMock.Setup(s => s.DeleteAsync(venda.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok()).Verifiable();
            var controller = new VendasController(_serviceMock.Object, _mapper);

            var res = await controller.Delete(venda.Id);

            Assert.IsType<NoContentResult>(res);
            _serviceMock.Verify(s => s.DeleteAsync(venda.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Excluir_DeveRetornarBadRequest_QuandoServicoRetornaFalha()
        {
            _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail("estorno inválido"));
            var controller = new VendasController(_serviceMock.Object, _mapper);

            var res = await controller.Delete(Guid.NewGuid()) as BadRequestObjectResult;

            Assert.NotNull(res);
            Assert.Contains("estorno inválido", res.Value!.ToString());
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarOk_QuandoEncontrado()
        {
            var venda = new Venda { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid(), ValorPago = 350000m, DataVenda = DateTime.UtcNow };
            _serviceMock.Setup(s => s.GetByIdAsync(venda.Id, It.IsAny<CancellationToken>())).ReturnsAsync(venda);
            var controller = new VendasController(_serviceMock.Object, _mapper);

            var result = await controller.Get(venda.Id);

            var ok = result.Result as OkObjectResult;
            Assert.NotNull(ok);
            Assert.NotNull(ok.Value);
        }
    }
}
