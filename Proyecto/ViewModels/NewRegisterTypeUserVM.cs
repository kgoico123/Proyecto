using Proyecto.Models;

namespace Proyecto.ViewModels;

public class NewRegisterTypeUserVM
{
    public string tipo { get; set; }
    public ApplicationUser User { get; set; }
    public Estudiante estudiante { get; set; }
    public Tutor tutor { get; set; }
    public Docente docente { get; set; }
    public Curso curso { get; set; }

    public IEnumerable<Curso> cursos { get; set; }
    public IEnumerable<Tutor> tutores { get; set; }
}
