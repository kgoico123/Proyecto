using System.Collections.Generic;
using Proyecto.Models;

namespace Proyecto.ViewModels;

public class TutorDashboardVM
{
    public IEnumerable<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();
}
