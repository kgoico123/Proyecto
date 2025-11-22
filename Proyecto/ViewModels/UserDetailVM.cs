using System.Collections.Generic;
using Proyecto.Models;

namespace Proyecto.ViewModels;

public class UserDetailVM
{
    public string UserId { get; set; } = string.Empty;
    public IEnumerable<string> role { get; set; } = new List<string>();
    public Tutor? tutor { get; set; }
    public Estudiante? estudiante { get; set; }
    public Docente? docente { get; set; }
    public ApplicationUser? Administrador { get; set; }
}
