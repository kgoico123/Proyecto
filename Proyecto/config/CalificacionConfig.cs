using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proyecto.Models;

namespace Proyecto.config;

public class CalificacionConfig : IEntityTypeConfiguration<Calificacion>
{
    public void Configure(EntityTypeBuilder<Calificacion> builder)
    {
        builder.ToTable("Calificacion");

        builder.HasKey(a => a.IdCalificacion);


        builder.Property(a => a.estudiante_CursoId)
            .IsRequired();

        builder.Property(a => a.FechaCalificacion)
            .IsRequired();

        builder.Property(a => a.Puntaje)
            .IsRequired();

        builder.Property(a => a.Comentario)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.promedioAcumulado)
            .IsRequired();
    }
}