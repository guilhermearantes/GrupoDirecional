using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infraestructure.Data.Configurations
{
    public class VendaConfiguration : IEntityTypeConfiguration<Venda>
    {
        public void Configure(EntityTypeBuilder<Venda> builder)
        {
            builder.ToTable("Vendas");
            builder.HasKey(v => v.Id);
            builder.Property(v => v.DataVenda).IsRequired();
            builder.Property(v => v.ValorPago).HasColumnType("decimal(18,2)");

            builder.HasOne(v => v.Cliente)
                .WithMany(c => c.Vendas)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Apartamento)
                .WithMany(a => a.Vendas)
                .HasForeignKey(v => v.ApartamentoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
