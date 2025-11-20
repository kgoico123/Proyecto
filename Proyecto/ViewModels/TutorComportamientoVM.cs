using Proyecto.Models;

namespace Proyecto.ViewModels;

public class TutorComportamientoVM
{
    public Estudiante Estudiante { get; set; }
    public IEnumerable<Comportamiento> Conductas { get; set; }
}
