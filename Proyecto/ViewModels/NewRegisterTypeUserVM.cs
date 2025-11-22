using System.Collections.Generic;
using Proyecto.Models;

namespace Proyecto.ViewModels;

public class NewRegisterTypeUserVM
{
    public string tipo { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public Estudiante? estudiante { get; set; }
    public Tutor? tutor { get; set; }
    public Docente? docente { get; set; }
    public Curso? curso { get; set; }

    public IEnumerable<Curso> cursos { get; set; } = new List<Curso>();
    public IEnumerable<Tutor> tutores { get; set; } = new List<Tutor>();
}
