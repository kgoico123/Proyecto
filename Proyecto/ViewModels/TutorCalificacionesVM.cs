using Proyecto.Models;

namespace Proyecto.ViewModels;

public class TutorCalificacionesVM
{
    public IEnumerable<PromedioCursoViewModel> PromediosPorCurso { get; set; }
    public IEnumerable<Calificacion> Calificaciones { get; set; }
    public Estudiante Estudiante { get; set; }
    public class PromedioCursoViewModel
    {
        public string Curso { get; set; }
        public double Promedio { get; set; }
    }
}
