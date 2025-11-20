
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proyecto.Models;

namespace Proyecto.config;

public class ComportamientoConfig : IEntityTypeConfiguration<Comportamiento>
{
    public void Configure(EntityTypeBuilder<Comportamiento> builder)
    {
        builder.ToTable("Comportamientos");

        builder.HasKey(a => a.IdComportamiento);

        builder.Property(a => a.estudiante_CursoId)
            .IsRequired();

        builder.Property(a => a.FechaRegistro)
            .IsRequired();

        builder.Property(a => a.Descripcion)
            .IsRequired()
            .HasMaxLength(500);


        builder.Property(a => a.Calificacion)
            .IsRequired()
            .HasMaxLength(500);


    }
}
