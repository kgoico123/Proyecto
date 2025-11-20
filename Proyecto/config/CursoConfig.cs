
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proyecto.Models;

namespace Proyecto.config;

public class CursoConfig : IEntityTypeConfiguration<Curso>
{
    public void Configure(EntityTypeBuilder<Curso> builder)
    {
        builder.ToTable("Cursos");

        builder.HasKey(a => a.IdCurso);

        builder.Property(a => a.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.HorarioInicio)
            .IsRequired();

        builder.Property(a => a.HorarioFin)
            .IsRequired();

        builder.Property(a => a.aula)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Grado)
            .IsRequired()
            .HasMaxLength(50);

        // hacemos la relacion con el docente pero cuando se elimina el curso no se elimina el docente solo se coloca a null el id
        builder.HasMany(a => a.Docentes)
            .WithOne(d => d.Curso)
            .HasForeignKey(a => a.CursoId)
            .OnDelete(DeleteBehavior.SetNull); // Cambiamos a SetNull para no eliminar el docente al eliminar el curso

    }
}
