using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infraestructure.Data
{
    public static class SeedData
    {
        public static void EnsureSeedData(AppDbContext context)
        {
            if (!context.Usuarios.Any())
            {
                var admin = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin"
                };
                context.Usuarios.Add(admin);
            }

            if (!context.Apartamentos.Any())
            {
                context.Apartamentos.Add(new Apartamento
                {
                    Id = Guid.NewGuid(),
                    Codigo = "A-101",
                    Bloco = "A",
                    Andar = 1,
                    Area = 75.5m,
                    Valor = 350000m,
                    Status = Domain.Enums.StatusApartamento.Disponivel
                });
            }

            if (!context.Clientes.Any())
            {
                context.Clientes.Add(new Cliente
                {
                    Id = Guid.NewGuid(),
                    Nome = "Cliente Demo",
                    Email = "demo@exemplo.com",
                    Cpf = "111.111.111-11",
                    DataNascimento = DateTime.UtcNow.AddYears(-30)
                });
            }

            context.SaveChanges();
        }
    }
}
