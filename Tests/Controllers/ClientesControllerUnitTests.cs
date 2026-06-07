using Moq;
using AutoMapper;
using DesafioTecnico.Api.Controllers;
using DesafioTecnico.Infrastructure.Repositories.Interfaces;
using DesafioTecnico.Infrastructure.Services.Interfaces;
using DesafioTecnico.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Tests.Controllers
{
    public class ClientesControllerUnitTests
    {
        private readonly Mock<IClienteService> _serviceMock;
        private readonly IMapper _mapper;

        public ClientesControllerUnitTests()
        {
            _serviceMock = new Mock<IClienteService>();
            var cfg = new MapperConfiguration(c => c.AddProfile<DesafioTecnico.Api.Mapping.AutoMapperProfile>());
            _mapper = cfg.CreateMapper();
        }

        [Fact]
        public async Task Post_Valid_ReturnsCreated()
        {
            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask).Verifiable();

            var controller = new ClientesController(_serviceMock.Object, _mapper);

            var dto = new DesafioTecnico.Api.DTOs.ClienteCreateDto { Nome = cliente.Nome, Email = cliente.Email, Cpf = cliente.Cpf, DataNascimento = cliente.DataNascimento };

            var result = await controller.Post(dto) as CreatedAtActionResult;

            Assert.NotNull(result);
            _serviceMock.Verify();
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);
            var controller = new ClientesController(_serviceMock.Object, _mapper);

            var res = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(res.Result);
        }

        [Fact]
        public async Task Put_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);
            var controller = new ClientesController(_serviceMock.Object, _mapper);

            var dto = new DesafioTecnico.Api.DTOs.ClienteUpdateDto { Nome = "x", Email = "a@b.com", Cpf = "123", DataNascimento = DateTime.UtcNow.AddYears(-30) };
            var res = await controller.Put(Guid.NewGuid(), dto);

            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public async Task Put_Existing_ReturnsNoContent()
        {
            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            _serviceMock.Setup(s => s.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask).Verifiable();

            var controller = new ClientesController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.ClienteUpdateDto { Nome = cliente.Nome, Email = cliente.Email, Cpf = cliente.Cpf, DataNascimento = cliente.DataNascimento };

            var res = await controller.Put(cliente.Id, dto);

            Assert.IsType<NoContentResult>(res);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Cliente>()), Times.Once);
        }
    }
}
