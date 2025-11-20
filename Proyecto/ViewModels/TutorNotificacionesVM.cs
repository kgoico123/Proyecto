using Proyecto.Models;

namespace Proyecto.ViewModels;

public class TutorNotificacionesVM
{
    public IEnumerable<Notificacion> Notificaciones { get; set; }
    public Estudiante Estudiante { get; set; }
}
