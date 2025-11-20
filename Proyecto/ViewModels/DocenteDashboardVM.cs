using Proyecto.Models;

namespace Proyecto.ViewModels;

public class DocenteDashboardVM
{
    public Docente docente { get; set; } = null!;
    public Curso curso { get; set; } = null!;
    public int CantidadAlumnos { get; set; }
}
