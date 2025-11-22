using Proyecto.Models;
using System.Collections.Generic;

namespace Proyecto.ViewModels;

public class DocenteCalificacionesVM
{
    public Docente docente { get; set; } = null!;
    public List<Secciones> secciones { get; set; } = new List<Secciones>();
    public string seccion { get; set; } = string.Empty;
    public List<int> alumnosId { get; set; } = new List<int>();
    public List<string> notas { get; set; } = new List<string>();
    public List<string> comentarios { get; set; } = new List<string>();

}

public class Secciones
{
    public string Grado { get; set; } = string.Empty;
    public string Seccion { get; set; } = string.Empty;
    public IEnumerable<AlumnoCalificacionVM> Alumnos { get; set; } = new List<AlumnoCalificacionVM>(); // Cambiado de Estudiante_Curso
}

// Nueva clase para representar los datos del alumno en esta vista
public class AlumnoCalificacionVM
{
    public int IdEstudiante { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}
