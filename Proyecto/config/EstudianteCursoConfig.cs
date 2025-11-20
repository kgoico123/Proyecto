
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proyecto.Models;

namespace Proyecto.config;

public class Estudiante_CursoConfig : IEntityTypeConfiguration<Estudiante_Curso>
{
    public void Configure(EntityTypeBuilder<Estudiante_Curso> builder)
    {
        builder.ToTable("Estudiante_Cursos");

        builder.HasKey(a => a.IdEstudianteCurso);

        builder.Property(a => a.EstudianteId)
            .IsRequired();

        builder.Property(a => a.CursoId)
            .IsRequired();

        builder.Property(a => a.FechaRegistro)
            .IsRequired();

        builder.HasMany(a => a.Calificaciones)
            .WithOne(e => e.Estudiante_Curso)
            .HasForeignKey(e => e.estudiante_CursoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
