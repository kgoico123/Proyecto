using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Proyecto.Models
{
    public class Estudiante
    {
        public int IdEstudiante { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser? user { get; set; }
        public int TutorId { get; set; }
        public Tutor Tutor { get; set; }
        public string Grado { get; set; }
        public IEnumerable<Estudiante_Curso> Estudiante_Cursos { get; set; } 
    }
}
