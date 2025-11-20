using System.ComponentModel.DataAnnotations;

namespace Proyecto.Models
{
    public class Curso
    {
        [Key]
        public int IdCurso { get; set; }

        public string Nombre { get; set; }
        // solo la hora
        public TimeSpan HorarioInicio { get; set; }
        public TimeSpan HorarioFin { get; set; }
        public string aula { get; set; }
        public string Grado { get; set; }

        public IEnumerable<Docente> Docentes { get; set; }

        public IEnumerable<Estudiante_Curso> estudiante_Curso { get; set; }
    }
}
