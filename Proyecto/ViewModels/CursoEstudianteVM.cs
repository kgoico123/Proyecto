using System.Collections.Generic;

namespace Proyecto.ViewModels
{
    public class CursoEstudianteVM
    {
        public int IdCurso { get; set; }
        public string NombreCurso { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;
        public string NombreDocente { get; set; } = string.Empty;
        public int? PromedioAcumulado { get; set; }
    }
}