using Moq;
using AutoMapper;
using DesafioTecnico.Api.Controllers;
using DesafioTecnico.Infrastructure.Services.Interfaces;
using DesafioTecnico.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Controllers
{
    public class ClientesControllerTests
    {
        private readonly Mock<IClienteService> _serviceMock;
        private readonly IMapper _mapper;

        public ClientesControllerTests()
        {
            _serviceMock = new Mock<IClienteService>();
            _mapper = Fixtures.MapperFactory.Create();
        }

        [Fact]
        public async Task Criar_DeveRetornarCreated_QuandoDadosValidos()
        {
            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>())).ReturnsAsync(cliente).Verifiable();

            var controller = new ClientesController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.ClienteCreateDto { Nome = cliente.Nome, Email = cliente.Email, Cpf = cliente.Cpf, DataNascimento = cliente.DataNascimento };

            var result = await controller.Post(dto) as CreatedAtActionResult;

            Assert.NotNull(result);
            _serviceMock.Verify();
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequest_QuandoModeloInvalido()
        {
            var controller = new ClientesController(_serviceMock.Object, _mapper);
            controller.ModelState.AddModelError("Nome", "Required");

            var result = await controller.Post(new DesafioTecnico.Api.DTOs.ClienteCreateDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
            var controller = new ClientesController(_serviceMock.Object, _mapper);

            var res = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(res.Result);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
            var controller = new ClientesController(_serviceMock.Object, _mapper);

            var dto = new DesafioTecnico.Api.DTOs.ClienteUpdateDto { Nome = "x", Email = "a@b.com", Cpf = "123", DataNascimento = DateTime.UtcNow.AddYears(-30) };
            var res = await controller.Put(Guid.NewGuid(), dto);

            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarNoContent_QuandoExiste()
        {
            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            _serviceMock.Setup(s => s.GetByIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var controller = new ClientesController(_serviceMock.Object, _mapper);
            var dto = new DesafioTecnico.Api.DTOs.ClienteUpdateDto { Nome = cliente.Nome, Email = cliente.Email, Cpf = cliente.Cpf, DataNascimento = cliente.DataNascimento };

            var res = await controller.Put(cliente.Id, dto);

            Assert.IsType<NoContentResult>(res);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Atualizar_DeveRetornarBadRequest_QuandoModeloInvalido()
        {
            var controller = new ClientesController(_serviceMock.Object, _mapper);
            controller.ModelState.AddModelError("Email", "Invalid");

            var res = await controller.Put(Guid.NewGuid(), new DesafioTecnico.Api.DTOs.ClienteUpdateDto());

            Assert.IsType<BadRequestObjectResult>(res);
        }

        [Fact]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
            var controller = new ClientesController(_serviceMock.Object, _mapper);

            var res = await controller.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public async Task Excluir_DeveRetornarNoContent_QuandoExiste()
        {
            var cliente = Fixtures.FakeDataBuilder.CreateCliente();
            _serviceMock.Setup(s => s.GetByIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
            _serviceMock.Setup(s => s.DeleteAsync(cliente.Id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var controller = new ClientesController(_serviceMock.Object, _mapper);
            var res = await controller.Delete(cliente.Id);

            Assert.IsType<NoContentResult>(res);
            _serviceMock.Verify(s => s.DeleteAsync(cliente.Id, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
