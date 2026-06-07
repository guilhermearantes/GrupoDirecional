namespace Tests.Fixtures
{
    internal class FakeDataBuilder
    {
        public static DesafioTecnico.Domain.Entities.Cliente CreateCliente()
        {
            var id = Guid.NewGuid();
            var digits = id.ToString("N")[..9];
            var cpf = $"{digits[..3]}.{digits[3..6]}.{digits[6..9]}-{digits[..2]}";
            return new DesafioTecnico.Domain.Entities.Cliente
            {
                Id = id,
                Nome = "Cliente Teste",
                Email = $"teste-{id:N}@exemplo.com",
                Cpf = cpf,
                DataNascimento = DateTime.UtcNow.AddYears(-30)
            };
        }

        public static DesafioTecnico.Domain.Entities.Apartamento CreateApartamento()
        {
            return new DesafioTecnico.Domain.Entities.Apartamento
            {
                Id = Guid.NewGuid(),
                Codigo = $"TEST-{Guid.NewGuid():N}"[..12],
                Bloco = "A",
                Andar = 1,
                Area = 75.5m,
                Valor = 350000m,
                Status = DesafioTecnico.Domain.Enums.StatusApartamento.Disponivel
            };
        }

        public static DesafioTecnico.Domain.Entities.Venda CreateVenda(Guid clienteId, Guid aptId)
        {
            return new DesafioTecnico.Domain.Entities.Venda
            {
                Id = Guid.NewGuid(),
                ClienteId = clienteId,
                ApartamentoId = aptId,
                DataVenda = DateTime.UtcNow,
                ValorPago = 100000m
            };
        }
    }
}
