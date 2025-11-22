using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto.Data;
using Proyecto.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Proyecto.shared;
using Proyecto.ViewModels;

namespace Proyecto.Controllers
{
    [Authorize(Roles = VCG.Role_Tutor)]
    public class TutorController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TutorController(AppDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard: muestra los estudiantes del tutor
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var tutor = await _context.Tutores
                .Include(t => t.Estudiantes!)
                    .ThenInclude(e => e.user)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            var vm = new TutorDashboardVM
            {
                Estudiantes = tutor?.Estudiantes?.ToList() ?? new List<Estudiante>()
            };

            return View(vm);
        }

        // Notificaciones: muestra las notificaciones del estudiante seleccionado
        public async Task<IActionResult> Notificaciones(int estudianteId)
        {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                var estudiante = await _context.Estudiantes
                .Include(e => e.user)
                .Include(e => e.Estudiante_Cursos!)
                .FirstOrDefaultAsync(e => e.IdEstudiante == estudianteId);

            if (estudiante == null)
            {
                return NotFound();
            }

            var notificaciones = await _context.Notificaciones
                .Where(n => n.TutorId == estudiante.TutorId && (n.Tutor != null && n.Tutor.Estudiantes != null && n.Tutor.Estudiantes.Any(e => e.IdEstudiante == estudiante.IdEstudiante)))
                .OrderByDescending(n => n.fecha)
                .ToListAsync();

            TutorNotificacionesVM responseVM = new TutorNotificacionesVM
            {
                Notificaciones = notificaciones,
                Estudiante = estudiante
            };
            return View(responseVM);
        }

        public async Task<IActionResult> LeerNotificacion(int notificacionId, [FromHeader(Name = "Referer")] string? referer = null)
        {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var notificacion = await _context.Notificaciones.FirstOrDefaultAsync(n => n.IdNotificacion == notificacionId);

            if (notificacion == null)
            {
                return NotFound(); // o redirecciona con un mensaje si prefieres
            }

            if (!notificacion.Leida)
            {
                notificacion.Leida = true;
                _context.Notificaciones.Update(notificacion);
                await _context.SaveChangesAsync();
            }

            // Prefer model binding for headers; fall back to typed headers if binding not provided.
            var resolvedReferer = referer ?? Request.GetTypedHeaders().Referer?.ToString();
            ViewBag.UrlAnterior = resolvedReferer;

            return View(notificacion);
        }

        // Conducta: muestra las conductas del estudiante seleccionado
        public async Task<IActionResult> Comportamiento(int estudianteId)
        {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
            var estudiante = await _context.Estudiantes
                .Include(e => e.user)
                .Include(e => e.Estudiante_Cursos)
                .FirstOrDefaultAsync(e => e.IdEstudiante == estudianteId);

            if (estudiante == null)
            {
                return NotFound();
            }

            // Obtener los ids de Estudiante_Curso de este estudiante
            var estudianteCursoIds = estudiante.Estudiante_Cursos?.Select(ec => ec.IdEstudianteCurso).ToList() ?? new List<int>();

            var conductas = await _context.Comportamientos
                .Where(c => estudianteCursoIds.Contains(c.estudiante_CursoId))
                .OrderByDescending(c => c.FechaRegistro)
                .ToListAsync();

            TutorComportamientoVM responseVM = new TutorComportamientoVM
            {
                Estudiante = estudiante,
                Conductas = conductas
            };
            return View(responseVM);
        }

        // Calificaciones: muestra las calificaciones del estudiante seleccionado
        public async Task<IActionResult> Calificaciones(int estudianteId)
        {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
            var estudiante = await _context.Estudiantes
                .Include(e => e.user)
                .Include(e => e.Estudiante_Cursos!)
                    .ThenInclude(ec => ec.Curso!)
                .FirstOrDefaultAsync(e => e.IdEstudiante == estudianteId);

            if (estudiante == null)
            {
                return NotFound();
            }

            var estudianteCursoIds = estudiante.Estudiante_Cursos?.Select(ec => ec.IdEstudianteCurso).ToList() ?? new List<int>();

            var calificaciones = await _context.Calificaciones
                .Include(c => c.Estudiante_Curso!)
                .Include(c => c.Estudiante_Curso!.Curso!)
                .Where(c => estudianteCursoIds.Contains(c.estudiante_CursoId))
                .OrderByDescending(c => c.FechaCalificacion)
                .ToListAsync();


            var promediosPorCurso = calificaciones
                .GroupBy(c => c.Estudiante_Curso?.Curso?.Nombre)
                .Where(g => g.Key != null)
                .Select(g =>
                {
                    var ultima = g.OrderByDescending(x => x.FechaCalificacion).FirstOrDefault();
                    return new TutorCalificacionesVM.PromedioCursoViewModel
                    {
                        Curso = g.Key!,
                        Promedio = ultima != null ? ultima.promedioAcumulado : 0
                    };
                })
                .ToList();

            TutorCalificacionesVM responseVM = new TutorCalificacionesVM
            {
                PromediosPorCurso = promediosPorCurso,
                Calificaciones = calificaciones,
                Estudiante = estudiante
            };
            return View(responseVM);
        }

    }
}
