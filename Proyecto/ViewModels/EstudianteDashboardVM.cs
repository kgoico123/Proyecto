using System.Collections.Generic;

namespace Proyecto.ViewModels
{
    public class EstudianteDashboardVM
    {
        
        public string NombreEstudiante { get; set; } = string.Empty;
        public List<CursoEstudianteVM> Cursos { get; set; } = new List<CursoEstudianteVM>();
    }
}