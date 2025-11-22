namespace Proyecto.Models;

public class Comportamiento
{
    public int IdComportamiento { get; set; }
    public int estudiante_CursoId { get; set; }
    public DateTime FechaRegistro { get; set; }
    public Estudiante_Curso? Estudiante_Curso { get; set; }
    public string? Calificacion { get; set; }
    public string? Descripcion { get; set; }
}
