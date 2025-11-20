using Microsoft.EntityFrameworkCore;
using Proyecto.Data;
using Proyecto.Models;
using Proyecto.shared;

namespace Proyecto.seed;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new AppDBContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<AppDBContext>>()))
        {
            context.Database.Migrate();

            if (!context.Cursos.Any())
            {
                var cursos = new List<Curso>();
                var grados = new[] { "Primero", "Segundo", "Tercero", "Cuarto", "Quinto" };
                var nombresCursos = new[]
                {
                    "Matemáticas", "Lengua y Literatura", "Ciencias Naturales", "Ciencias Sociales", "Inglés",
                    "Educación Física", "Arte y Cultura", "Música", "Tecnología", "Historia"
                };
                var pabellones = new[] { "A", "B", "C", "D", "E" };

                for (int gradoIndex = 0; gradoIndex < grados.Length; gradoIndex++)
                {
                    var grado = grados[gradoIndex];
                    var pabellon = pabellones[gradoIndex];
                    int numeroBaseAula = (gradoIndex + 1) * 100;

                    for (int i = 0; i < nombresCursos.Length; i++)
                    {
                        var horaInicio = 8 + i;
                        var numeroAula = numeroBaseAula + i + 1;

                        cursos.Add(new Curso
                        {
                            Nombre = $"{nombresCursos[i]}",
                            aula = $"{pabellon}-{numeroAula}",
                            Grado = grado,
                            HorarioInicio = new TimeSpan(horaInicio, 0, 0),
                            HorarioFin = new TimeSpan(horaInicio, 45, 0)
                        });
                    }
                }
                context.Cursos.AddRange(cursos);
                context.SaveChanges();
            }
        }
    }
}