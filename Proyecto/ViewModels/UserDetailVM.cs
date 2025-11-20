using Proyecto.Models;

namespace Proyecto.ViewModels;

public class UserDetailVM
{
    public string UserId { get; set; }
    public IEnumerable<string> role { get; set; }
    public Tutor tutor { get; set; }
    public Estudiante estudiante { get; set; }
    public Docente docente { get; set; }
    public ApplicationUser Administrador { get; set; }
}
