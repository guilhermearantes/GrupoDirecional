using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DesafioTecnico.Domain.Entities;

namespace DesafioTecnico.Infrastructure.Data.Configurations
{
    public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
    {
        public void Configure(EntityTypeBuilder<Reserva> builder)
        {
            builder.ToTable("Reservas");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.DataReserva).IsRequired();
            builder.Property(r => r.Status).IsRequired();

            builder.HasOne(r => r.Cliente)
                .WithMany(c => c.Reservas)
                .HasForeignKey(r => r.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Apartamento)
                .WithMany(a => a.Reservas)
                .HasForeignKey(r => r.ApartamentoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
