using Proyecto.Models;

namespace Proyecto.ViewModels;

public class DocenteConductaVM
{
    public Docente Docente { get; set; }
    public Curso Curso { get; set; }
    public List<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();
    public List<int> AlumnosId { get; set; } = new List<int>();
    public List<string> Conductas { get; set; } = new List<string>();
    public List<string> Comentarios { get; set; } = new List<string>();
}
