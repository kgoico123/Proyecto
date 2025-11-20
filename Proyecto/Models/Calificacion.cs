using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Xml;

namespace Proyecto.Models
{
    public class Calificacion
    {
        public int IdCalificacion { get; set; }
        public int estudiante_CursoId { get; set; }
        public Estudiante_Curso Estudiante_Curso { get; set; }
        public DateTime FechaCalificacion { get; set; }
        public int Puntaje { get; set; }
        public string Comentario { get; set; }
        public int promedioAcumulado { get; set; }
    }
}
