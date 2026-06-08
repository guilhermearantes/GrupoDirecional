using Moq;
using AutoMapper;
using DesafioTecnico.Api.Controllers;
using DesafioTecnico.Infrastructure.Services.Interfaces;
using DesafioTecnico.Domain.Entities;
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
            var cfg = new MapperConfiguration(c => c.AddProfile<DesafioTecnico.Api.Mapping.AutoMapperProfile>());
            _mapper = cfg.CreateMapper();
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
        public async Task Criar_DeveRetornarBadRequest_QuandoServicoLancaExcecao()
        {
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Venda>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("nope"));
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
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Venda>(), It.IsAny<CancellationToken>())).ReturnsAsync(venda);

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
    }
}
