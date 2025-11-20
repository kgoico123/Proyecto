using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proyecto.Models;

namespace Proyecto.config;

public class EstudianteConfig : IEntityTypeConfiguration<Estudiante>
{
    public void Configure(EntityTypeBuilder<Estudiante> builder)
    {
        builder.ToTable("Estudiantes");

        builder.HasKey(a => a.IdEstudiante);

        builder.Property(a => a.Grado)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.TutorId)
            .IsRequired();

        builder.HasMany(a => a.Estudiante_Cursos)
            .WithOne(e => e.Estudiante)
            .HasForeignKey(e => e.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}