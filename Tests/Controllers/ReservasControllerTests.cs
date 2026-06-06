using Moq;
using AutoMapper;
using DesafioTecnico.Api.Controllers;
using DesafioTecnico.Infraestructure.Services.Interfaces;
using DesafioTecnico.Domain.Entities;
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
            var cfg = new MapperConfiguration(c => c.AddProfile<DesafioTecnico.Api.Mapping.AutoMapperProfile>());
            _mapper = cfg.CreateMapper();
        }

        [Fact]
        public async Task Post_InvalidModel_ReturnsBadRequest()
        {
            var controller = new ReservasController(_serviceMock.Object, _mapper);
            controller.ModelState.AddModelError("ClienteId", "Required");

            var dto = new DesafioTecnico.Api.DTOs.ReservaCreateDto();
            var result = await controller.Post(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Post_Valid_ReturnsCreated()
        {
            var reserva = new Reserva { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid(), DataReserva = DateTime.UtcNow, Status = DesafioTecnico.Domain.Enums.StatusReserva.Pendente };
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Reserva>())).ReturnsAsync(reserva);

            var controller = new ReservasController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.ReservaCreateDto { ClienteId = reserva.ClienteId, ApartamentoId = reserva.ApartamentoId };

            var res = await controller.Post(dto) as CreatedAtActionResult;

            Assert.NotNull(res);
            Assert.Equal("Get", res.ActionName);
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Reserva?)null);
            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Get(Guid.NewGuid());
            Assert.IsType<NotFoundResult>(res.Result);
        }

        [Fact]
        public async Task Confirm_ServiceThrows_ReturnsBadRequest()
        {
            _serviceMock.Setup(s => s.ConfirmAsync(It.IsAny<Guid>())).ThrowsAsync(new InvalidOperationException("cannot confirm"));
            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Confirm(Guid.NewGuid()) as BadRequestObjectResult;

            Assert.NotNull(res);
            Assert.Contains("cannot confirm", res.Value.ToString());
        }

        [Fact]
        public async Task Confirm_Succeeds_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.ConfirmAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Confirm(Guid.NewGuid());
            Assert.IsType<NoContentResult>(res);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Reserva?)null);
            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Delete(Guid.NewGuid());
            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            var reserva = new Reserva { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ApartamentoId = Guid.NewGuid() };
            _serviceMock.Setup(s => s.GetByIdAsync(reserva.Id)).ReturnsAsync(reserva);
            _serviceMock.Setup(s => s.DeleteAsync(reserva.Id)).Returns(Task.CompletedTask).Verifiable();

            var controller = new ReservasController(_serviceMock.Object, _mapper);

            var res = await controller.Delete(reserva.Id);

            Assert.IsType<NoContentResult>(res);
            _serviceMock.Verify();
        }
    }
}
