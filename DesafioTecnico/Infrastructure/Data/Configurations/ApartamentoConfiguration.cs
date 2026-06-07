using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Data.Configurations
{
    public class ApartamentoConfiguration : IEntityTypeConfiguration<Apartamento>
    {
        public void Configure(EntityTypeBuilder<Apartamento> builder)
        {
            builder.ToTable("Apartamentos");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Codigo).HasMaxLength(50).IsRequired();
            builder.Property(a => a.Bloco).HasMaxLength(50);
            builder.Property(a => a.Andar).IsRequired();
            builder.Property(a => a.Area).HasColumnType("decimal(10,2)");
            builder.Property(a => a.Valor).HasColumnType("decimal(18,2)");

            builder.HasIndex(a => a.Codigo).IsUnique();
        }
    }
}
