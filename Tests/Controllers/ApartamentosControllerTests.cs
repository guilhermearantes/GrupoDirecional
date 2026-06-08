using Moq;
using Microsoft.AspNetCore.Mvc;
using DesafioTecnico.Api.Controllers;
using DesafioTecnico.Infrastructure.Services.Interfaces;
using DesafioTecnico.Domain.Entities;
using AutoMapper;

namespace Tests.Controllers
{
    public class ApartamentosControllerTests
    {
        private readonly Mock<IApartamentoService> _serviceMock;
        private readonly IMapper _mapper;

        public ApartamentosControllerTests()
        {
            _serviceMock = new Mock<IApartamentoService>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<DesafioTecnico.Api.Mapping.AutoMapperProfile>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Apartamento?)null);
            var controller = new ApartamentosController(_serviceMock.Object, _mapper);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequest_QuandoModeloInvalido()
        {
            var controller = new ApartamentosController(_serviceMock.Object, _mapper);
            controller.ModelState.AddModelError("Codigo", "Required");

            var result = await controller.Post(new DesafioTecnico.Api.DTOs.ApartamentoCreateDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Criar_DeveRetornarCreated_QuandoDadosValidos()
        {
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Apartamento>(), It.IsAny<CancellationToken>())).ReturnsAsync(apt).Verifiable();

            var controller = new ApartamentosController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.ApartamentoCreateDto { Codigo = apt.Codigo, Bloco = apt.Bloco, Andar = apt.Andar, Area = apt.Area, Valor = apt.Valor };

            var result = await controller.Post(dto) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal("Get", result.ActionName);
            _serviceMock.Verify(s => s.CreateAsync(It.IsAny<Apartamento>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Apartamento?)null);
            var controller = new ApartamentosController(_serviceMock.Object, _mapper);

            var res = await controller.Put(Guid.NewGuid(), new DesafioTecnico.Api.DTOs.ApartamentoUpdateDto());

            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarNoContent_QuandoExiste()
        {
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            _serviceMock.Setup(s => s.GetByIdAsync(apt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(apt);
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Apartamento>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var controller = new ApartamentosController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.ApartamentoUpdateDto { Codigo = apt.Codigo, Bloco = apt.Bloco, Andar = apt.Andar, Area = apt.Area, Valor = apt.Valor };

            var res = await controller.Put(apt.Id, dto);

            Assert.IsType<NoContentResult>(res);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Apartamento>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarBadRequest_QuandoModeloInvalido()
        {
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            var controller = new ApartamentosController(_serviceMock.Object, _mapper);
            controller.ModelState.AddModelError("Andar", "Required");

            var res = await controller.Put(apt.Id, new DesafioTecnico.Api.DTOs.ApartamentoUpdateDto()) as BadRequestObjectResult;

            Assert.NotNull(res);
        }

        [Fact]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Apartamento?)null);
            var controller = new ApartamentosController(_serviceMock.Object, _mapper);

            var res = await controller.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public async Task Excluir_DeveRetornarNoContent_QuandoExiste()
        {
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            _serviceMock.Setup(s => s.GetByIdAsync(apt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(apt);
            _serviceMock.Setup(s => s.DeleteAsync(apt.Id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var controller = new ApartamentosController(_serviceMock.Object, _mapper);
            var res = await controller.Delete(apt.Id);

            Assert.IsType<NoContentResult>(res);
            _serviceMock.Verify(s => s.DeleteAsync(apt.Id, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
