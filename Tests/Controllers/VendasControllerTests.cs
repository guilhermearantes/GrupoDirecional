using Moq;
using AutoMapper;
using DesafioTecnico.Api.Controllers;
using DesafioTecnico.Infraestructure.Services.Interfaces;
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
        public async Task Post_InvalidModel_ReturnsBadRequest()
        {
            var controller = new VendasController(_serviceMock.Object, _mapper);
            controller.ModelState.AddModelError("ClienteId", "Required");

            var dto = new DesafioTecnico.Api.DTOs.VendaCreateDto();
            var result = await controller.Post(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Post_ServiceThrows_ReturnsBadRequest()
        {
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<DesafioTecnico.Domain.Entities.Venda>())).ThrowsAsync(new InvalidOperationException("nope"));
            var controller = new VendasController(_serviceMock.Object, _mapper);

            var dto = new DesafioTecnico.Api.DTOs.VendaCreateDto { ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid(), ValorPago = 100m };
            var res = await controller.Post(dto) as BadRequestObjectResult;

            Assert.NotNull(res);
            Assert.Contains("nope", res.Value.ToString());
        }

        [Fact]
        public async Task Post_Valid_ReturnsCreated()
        {
            var venda = new DesafioTecnico.Domain.Entities.Venda { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid(), ValorPago = 100m, DataVenda = DateTime.UtcNow /* UTC */ };
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<DesafioTecnico.Domain.Entities.Venda>())).ReturnsAsync(venda);

            var controller = new VendasController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.VendaCreateDto { ClienteId = venda.ClienteId, ApartamentoId = venda.ApartamentoId, ValorPago = venda.ValorPago };

            var res = await controller.Post(dto) as CreatedAtActionResult;

            Assert.NotNull(res);
            Assert.Equal("Get", res.ActionName);
        }

        [Fact]
        public async Task Put_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((DesafioTecnico.Domain.Entities.Venda?)null);
            var controller = new VendasController(_serviceMock.Object, _mapper);

            var dto = new DesafioTecnico.Api.DTOs.VendaUpdateDto { ValorPago = 200m };
            var res = await controller.Put(Guid.NewGuid(), dto);

            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public async Task Put_Existing_ReturnsNoContent()
        {
            var venda = new DesafioTecnico.Domain.Entities.Venda { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid(), ValorPago = 100m, DataVenda = DateTime.UtcNow };
            _serviceMock.Setup(s => s.GetByIdAsync(venda.Id)).ReturnsAsync(venda);
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<DesafioTecnico.Domain.Entities.Venda>())).Returns(Task.CompletedTask).Verifiable();

            var controller = new VendasController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.VendaUpdateDto { ValorPago = 250m };

            var res = await controller.Put(venda.Id, dto);

            Assert.IsType<NoContentResult>(res);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<DesafioTecnico.Domain.Entities.Venda>()), Times.Once);
        }

        [Fact]
        public async Task Put_InvalidModel_ReturnsBadRequest()
        {
            var venda = new DesafioTecnico.Domain.Entities.Venda { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid(), ValorPago = 100m, DataVenda = DateTime.UtcNow };
            var controller = new VendasController(_serviceMock.Object, _mapper);
            controller.ModelState.AddModelError("ValorPago", "Required");

            var dto = new DesafioTecnico.Api.DTOs.VendaUpdateDto();
            var res = await controller.Put(venda.Id, dto) as BadRequestObjectResult;

            Assert.NotNull(res);
        }
    }
}
