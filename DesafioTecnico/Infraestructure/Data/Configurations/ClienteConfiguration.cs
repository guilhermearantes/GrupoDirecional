using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infraestructure.Data.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Clientes");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Nome).HasMaxLength(200).IsRequired();
            builder.Property(c => c.Email).HasMaxLength(200).IsRequired();
            builder.Property(c => c.Cpf).HasMaxLength(14).IsRequired();
            builder.Property(c => c.DataNascimento).IsRequired();

            builder.HasIndex(c => c.Email).IsUnique();
            builder.HasIndex(c => c.Cpf).IsUnique();
        }
    }
}
