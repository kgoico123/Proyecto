namespace Proyecto.Models;

public class Estudiante_Curso
{
    public int IdEstudianteCurso { get; set; }
    public int EstudianteId { get; set; }
    public Estudiante? Estudiante { get; set; }

    public int CursoId { get; set; }
    public Curso? Curso { get; set; }
    public DateTime FechaRegistro { get; set; }

    public IEnumerable<Calificacion>? Calificaciones { get; set; }
}
