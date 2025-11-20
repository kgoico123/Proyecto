using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proyecto.Models;

namespace Proyecto.config;

public class TutorConfig : IEntityTypeConfiguration<Tutor>
{
    public void Configure(EntityTypeBuilder<Tutor> builder)
    {
        builder.ToTable("Tutores");

        builder.HasKey(a => a.IdTutor);

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.HasMany(a => a.Estudiantes)
            .WithOne(e => e.Tutor)
            .HasForeignKey(e => e.TutorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Notificaciones)
            .WithOne(r => r.Tutor)
            .HasForeignKey(r => r.TutorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}