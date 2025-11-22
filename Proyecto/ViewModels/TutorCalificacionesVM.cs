using System.Collections.Generic;
using Proyecto.Models;

namespace Proyecto.ViewModels;

public class TutorCalificacionesVM
{
    public IEnumerable<PromedioCursoViewModel> PromediosPorCurso { get; set; } = new List<PromedioCursoViewModel>();
    public IEnumerable<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
    public Estudiante? Estudiante { get; set; }
    public class PromedioCursoViewModel
    {
        public string Curso { get; set; } = string.Empty;
        public double Promedio { get; set; }
    }
}
