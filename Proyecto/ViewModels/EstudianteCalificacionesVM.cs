using System.Collections.Generic;
using Proyecto.Models;

namespace Proyecto.ViewModels
{
    public class EstudianteCalificacionesVM
    {
        public string NombreCurso { get; set; }
        public string NombreDocente { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;
        public List<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
        public int? promedioAcumulado { get; set; }
    }
}