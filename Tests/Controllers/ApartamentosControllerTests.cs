using Moq;
using Microsoft.AspNetCore.Mvc;
using DesafioTecnico.Api.Controllers;
using DesafioTecnico.Infraestructure.Services.Interfaces;
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
            var config = new MapperConfiguration(cfg => cfg.AddProfile<DesafioTecnico.Api.Mapping.AutoMapperProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNull()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Apartamento?)null);

            var controller = new ApartamentosController(_serviceMock.Object, _mapper);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Post_InvalidModel_ReturnsBadRequest()
        {
            var controller = new ApartamentosController(_serviceMock.Object, _mapper);
            controller.ModelState.AddModelError("Codigo", "Required");

            var dto = new DesafioTecnico.Api.DTOs.ApartamentoCreateDto();
            var result = await controller.Post(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Post_Valid_ReturnsCreated()
        {
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Apartamento>())).ReturnsAsync(apt).Verifiable();

            var controller = new ApartamentosController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.ApartamentoCreateDto { Codigo = apt.Codigo, Bloco = apt.Bloco, Andar = apt.Andar, Area = apt.Area, Valor = apt.Valor };

            var result = await controller.Post(dto) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal("Get", result.ActionName);
            _serviceMock.Verify(s => s.CreateAsync(It.IsAny<Apartamento>()), Times.Once);
        }

        [Fact]
        public async Task Put_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Apartamento?)null);
            var controller = new ApartamentosController(_serviceMock.Object, _mapper);

            var dto = new DesafioTecnico.Api.DTOs.ApartamentoUpdateDto();
            var res = await controller.Put(Guid.NewGuid(), dto);

            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public async Task Put_Existing_ReturnsNoContent()
        {
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            _serviceMock.Setup(s => s.GetByIdAsync(apt.Id)).ReturnsAsync(apt);
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Apartamento>())).Returns(Task.CompletedTask).Verifiable();

            var controller = new ApartamentosController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.ApartamentoUpdateDto { Codigo = apt.Codigo, Bloco = apt.Bloco, Andar = apt.Andar, Area = apt.Area, Valor = apt.Valor };

            var res = await controller.Put(apt.Id, dto);

            Assert.IsType<NoContentResult>(res);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Apartamento>()), Times.Once);
        }

        [Fact]
        public async Task Put_InvalidModel_ReturnsBadRequest()
        {
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            var controller = new ApartamentosController(_serviceMock.Object, _mapper);
            controller.ModelState.AddModelError("Andar", "Required");

            var dto = new DesafioTecnico.Api.DTOs.ApartamentoUpdateDto();
            var res = await controller.Put(apt.Id, dto) as BadRequestObjectResult;

            Assert.NotNull(res);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Apartamento?)null);
            var controller = new ApartamentosController(_serviceMock.Object, _mapper);

            var res = await controller.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            var apt = Fixtures.FakeDataBuilder.CreateApartamento();
            _serviceMock.Setup(s => s.GetByIdAsync(apt.Id)).ReturnsAsync(apt);
            _serviceMock.Setup(s => s.DeleteAsync(apt.Id)).Returns(Task.CompletedTask).Verifiable();

            var controller = new ApartamentosController(_serviceMock.Object, _mapper);

            var res = await controller.Delete(apt.Id);

            Assert.IsType<NoContentResult>(res);
            _serviceMock.Verify(s => s.DeleteAsync(apt.Id), Times.Once);
        }
    }
}
