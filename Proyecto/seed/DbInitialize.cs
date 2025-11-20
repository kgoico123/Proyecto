using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proyecto.Data;
using Proyecto.Models;
using Proyecto.shared;

namespace Proyecto.seed;

public class DbInitialize : IDbInitialize
{
    private readonly AppDBContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public DbInitialize(AppDBContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager; 
        _roleManager = roleManager;
    }
    public void Initialize()
    {
        // Make sure database schema is applied before querying Identity tables
        try
        {
            _context.Database.Migrate();
        }
        catch (Exception)
        {
            // If migrations cannot be applied for some reason, attempt to create the database
            // to avoid throwing during startup when tables like AspNetRoles don't exist.
            try
            {
                _context.Database.EnsureCreated();
            }
            catch
            {
                // If EnsureCreated also fails, stop initialization to avoid masking the underlying issue.
                return;
            }
        }

        if (!_context.Database.CanConnect()) return;

        // Comprobar si la tabla AspNetRoles existe; si la consulta falla, intentar crearla.
        bool rolesExist = false;
        try
        {
            rolesExist = _context.Roles.Any();
        }
        catch (Exception)
        {
            // Si la consulta falla (tabla faltante), intentamos recrear la base de datos completamente
            try
            {
                _context.Database.EnsureDeleted();
            }
            catch
            {
                // Ignorar errores al borrar
            }

            try
            {
                _context.Database.EnsureCreated();
            }
            catch
            {
                // Ignorar errores; rolesExist quedará en false y seguiremos intentando crear los roles.
            }

            try
            {
                rolesExist = _context.Roles.Any();
            }
            catch
            {
                rolesExist = false;
            }
        }

        if (rolesExist) return;

        // Asegurar que las tablas se crean antes de usar RoleManager (evita Invalid object name)
        try
        {
            _context.Database.EnsureCreated();
        }
        catch
        {
            // Si falla, continuamos y dejamos que las llamadas siguientes manejen el error.
        }

        _roleManager.CreateAsync(new IdentityRole(VCG.Role_Admin)).GetAwaiter().GetResult();
        _roleManager.CreateAsync(new IdentityRole(VCG.Role_Estudiante)).GetAwaiter().GetResult();
        _roleManager.CreateAsync(new IdentityRole(VCG.Role_Tutor)).GetAwaiter().GetResult();
        _roleManager.CreateAsync(new IdentityRole(VCG.Role_Docente)).GetAwaiter().GetResult();

        var admin = new ApplicationUser
        {
            Email = "admin@dev.cs",
            UserName = "admin@dev.cs",
            PhoneNumber = "123456789",
            Dni = "76543110",
        };

        var estudiante = new ApplicationUser
        {
            Email = "estudiante@dev.cs",
            UserName = "estudiante@dev.cs",
            PhoneNumber = "123456789",
            Dni = "76543330",
        };

        var tutor = new ApplicationUser
        {
            Email = "tutor@dev.cs",
            UserName = "tutor@dev.cs",
            PhoneNumber = "123456789",
            Dni = "76544440",
        };

        var docente = new ApplicationUser
        {
            Email = "docente@dev.cs",
            UserName = "docente@dev.cs",
            PhoneNumber = "123456789",
            Dni = "76543220",
        };

        _userManager.CreateAsync(admin, "Admin123*").GetAwaiter().GetResult();
        _userManager.CreateAsync(estudiante, "Estudiante123*").GetAwaiter().GetResult();
        _userManager.CreateAsync(tutor, "Tutor123*").GetAwaiter().GetResult();
        _userManager.CreateAsync(docente, "Docente123*").GetAwaiter().GetResult();

        _userManager.AddToRoleAsync(admin, VCG.Role_Admin).GetAwaiter().GetResult();
        _userManager.AddToRoleAsync(tutor, VCG.Role_Tutor).GetAwaiter().GetResult();
        _userManager.AddToRoleAsync(estudiante, VCG.Role_Estudiante).GetAwaiter().GetResult();
        _userManager.AddToRoleAsync(docente, VCG.Role_Docente).GetAwaiter().GetResult();

        if (!_context.Tutores.Any() && !_context.Estudiantes.Any() && !_context.Docentes.Any())
        {
            var nuevoTutor = new Tutor { UserId = tutor.Id, direccion = "Calle Falsa 123" };
            _context.Tutores.Add(nuevoTutor);
            _context.SaveChanges();

            var nuevoEstudiante = new Estudiante { UserId = estudiante.Id, TutorId = nuevoTutor.IdTutor, Grado = Grados.Primero };
            _context.Estudiantes.Add(nuevoEstudiante);

            var nuevoDocente = new Docente { UserId = docente.Id };
            _context.Docentes.Add(nuevoDocente);

            _context.SaveChanges();
        }
    }
}
