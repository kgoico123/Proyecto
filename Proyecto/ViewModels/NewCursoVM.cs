using System.Collections.Generic;
using Proyecto.Models;

namespace Proyecto.ViewModels;

public class NewCursoVM
{
    public Curso? Curso { get; set; }
    public IEnumerable<Docente> Docentes { get; set; } = new List<Docente>();
    public int DocenteId { get; set; }
}
