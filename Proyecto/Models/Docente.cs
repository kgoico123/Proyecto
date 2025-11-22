using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Identity;

namespace Proyecto.Models
{
    public class Docente
    {
        public int IdDocente { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? user { get; set; }
        public int? CursoId { get; set; }
        public Curso? Curso { get; set; }
       
    }
}
