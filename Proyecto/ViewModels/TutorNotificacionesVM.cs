using System.Collections.Generic;
using Proyecto.Models;

namespace Proyecto.ViewModels;

public class TutorNotificacionesVM
{
    public IEnumerable<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    public Estudiante? Estudiante { get; set; }
}
