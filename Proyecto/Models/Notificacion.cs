using System.ComponentModel.DataAnnotations;

namespace Proyecto.Models
{
    public class Notificacion
    {
        public int IdNotificacion { get; set; }
        public int TutorId { get; set; }
        public Tutor? Tutor { get; set; }
        public DateTime fecha { get; set; }
        public string? Titulo { get; set; }
        public string? Mensaje { get; set; }
        public bool Leida { get; set; }
        public string? Tipo { get; set; }
    }
}
