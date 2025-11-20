using Proyecto.Models;

namespace Proyecto.ViewModels;

public class NewCursoVM
{
    public Curso Curso { get; set; }
    public IEnumerable<Docente> Docentes { get; set; }
    public int DocenteId { get; set; }
}
