using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto.Data;
using Proyecto.Models;
using Proyecto.shared;
using Proyecto.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Proyecto.Controllers
{
    [Authorize(Roles = VCG.Role_Estudiante)]
    public class EstudianteController : Controller
    {
        private const string TempErrorKey = "ErrorMessage";

        private readonly AppDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EstudianteController(AppDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            // 1. Obtener el usuario actual y su entidad Estudiante
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var estudiante = await _context.Estudiantes
                .Include(e => e.user)
                .Include(e => e.Estudiante_Cursos!)
                    .ThenInclude(ec => ec.Curso!)
                        .ThenInclude(c => c.Docentes!)
                            .ThenInclude(d => d.user)
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (estudiante == null)
            {
                return View(new EstudianteDashboardVM()); // Devuelve un modelo vacío si no se encuentra
            }

            var viewModel = new EstudianteDashboardVM
            {
                NombreEstudiante = estudiante.user?.UserName ?? "Estudiante",
                Cursos = new List<CursoEstudianteVM>()
            };

            // 2. Iterar sobre cada curso en el que el estudiante está inscrito
            foreach (var ec in estudiante.Estudiante_Cursos ?? Enumerable.Empty<Estudiante_Curso>())
            {
                // 3. Obtener la última calificación para sacar el promedio acumulado
                var ultimaCalificacion = await _context.Calificaciones
                    .Where(c => c.estudiante_CursoId == ec.IdEstudianteCurso)
                    .OrderByDescending(c => c.FechaCalificacion)
                    .FirstOrDefaultAsync();

                var docentesNombres = ec.Curso?.Docentes?.Select(d => d.user?.UserName).Where(n => !string.IsNullOrEmpty(n)).ToList();
                string horario = "";
                if (ec.Curso != null)
                {
                    horario = $"{ec.Curso.HorarioInicio:hh\\:mm} - {ec.Curso.HorarioFin:hh\\:mm}";
                }

                var cursoVM = new CursoEstudianteVM
                {
                    IdCurso = ec.Curso?.IdCurso ?? 0,
                    NombreCurso = ec.Curso?.Nombre ?? "Desconocido",
                    Aula = ec.Curso?.aula ?? string.Empty,
                    Horario = horario,
                    // Unir los nombres de los docentes asignados a ese curso
                    NombreDocente = (docentesNombres != null && docentesNombres.Count > 0)
                                    ? string.Join(", ", docentesNombres)
                                    : "No asignado",
                    PromedioAcumulado = ultimaCalificacion?.promedioAcumulado
                };
                viewModel.Cursos.Add(cursoVM);
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Calificaciones(int cursoId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var estudiante = await _context.Estudiantes.FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (estudiante == null)
            {
                return NotFound("Estudiante no encontrado.");
            }

            // Buscar la inscripción específica del estudiante en ese curso
            var estudianteCurso = await _context.Estudiantes_Cursos
                .Include(ec => ec.Curso!)
                    .ThenInclude(c => c.Docentes!)
                        .ThenInclude(d => d.user)
                .Include(ec => ec.Calificaciones)
                .FirstOrDefaultAsync(ec => ec.EstudianteId == estudiante.IdEstudiante && ec.CursoId == cursoId);

            if (estudianteCurso == null)
            {
                return NotFound("No estás inscrito en este curso o el curso no existe.");
            }

            // Obtener la última calificación para el promedio
            var ultimaCalificacion = (estudianteCurso.Calificaciones ?? Enumerable.Empty<Calificacion>())
                .OrderByDescending(c => c.FechaCalificacion)
                .FirstOrDefault();

            var docentesList = estudianteCurso.Curso?.Docentes?.Select(d => d.user?.UserName).Where(n => !string.IsNullOrEmpty(n)).ToList();
            var viewModel = new EstudianteCalificacionesVM
            {
                NombreCurso = estudianteCurso.Curso?.Nombre ?? "Desconocido",
                NombreDocente = (docentesList != null && docentesList.Count > 0)
                                ? string.Join(", ", docentesList)
                                : "No asignado",
                Horario = estudianteCurso.Curso != null ? $"{estudianteCurso.Curso.HorarioInicio:hh\\:mm} - {estudianteCurso.Curso.HorarioFin:hh\\:mm}" : string.Empty,
                Calificaciones = (estudianteCurso.Calificaciones ?? Enumerable.Empty<Calificacion>()).OrderBy(c => c.FechaCalificacion).ToList(),
                promedioAcumulado = ultimaCalificacion?.promedioAcumulado
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Cursos()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var estudiante = await _context.Estudiantes
                .Include(e => e.Estudiante_Cursos)
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (estudiante == null)
            {
                return NotFound("Estudiante no encontrado.");
            }

            // Obtener los IDs de los cursos en los que el estudiante ya está inscrito
            var idsCursosInscritos = estudiante.Estudiante_Cursos?.Select(ec => ec.CursoId).ToList() ?? new List<int>();

            // Obtener todos los cursos disponibles para el grado del estudiante, excluyendo en los que ya está inscrito
            var cursosDisponibles = await _context.Cursos
                .Where(c => c.Grado == estudiante.Grado && !idsCursosInscritos.Contains(c.IdCurso))
                .ToListAsync();

            var viewModel = new InscripcionCursoVM
            {
                CursosDisponibles = cursosDisponibles
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cursos(int cursoId)
        {
            if (!ModelState.IsValid)
            {
                TempData[TempErrorKey] = "Solicitud inválida.";
                return RedirectToAction(nameof(Cursos));
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData[TempErrorKey] = "No se pudo identificar al usuario.";
                return RedirectToAction(nameof(Cursos));
            }
            var estudiante = await _context.Estudiantes.FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (estudiante == null)
            {
                TempData[TempErrorKey] = "No se pudo encontrar tu perfil de estudiante.";
                return RedirectToAction(nameof(Cursos));
            }

            // Verificar si el curso existe y corresponde al grado del estudiante
            var cursoAInscribir = await _context.Cursos.FindAsync(cursoId);
            if (cursoAInscribir == null || cursoAInscribir.Grado != estudiante.Grado)
            {
                TempData[TempErrorKey] = "El curso seleccionado no es válido o no corresponde a tu grado.";
                return RedirectToAction(nameof(Cursos));
            }

            // Verificar si ya está inscrito
            var yaInscrito = await _context.Estudiantes_Cursos
                .AnyAsync(ec => ec.EstudianteId == estudiante.IdEstudiante && ec.CursoId == cursoId);

            if (yaInscrito)
            {
                TempData[TempErrorKey] = "Ya te encuentras inscrito en este curso.";
                return RedirectToAction(nameof(Cursos));
            }

            // Crear la nueva inscripción
            var nuevaInscripcion = new Estudiante_Curso
            {
                EstudianteId = estudiante.IdEstudiante,
                CursoId = cursoId
            };

            _context.Estudiantes_Cursos.Add(nuevaInscripcion);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Te has inscrito correctamente en el curso '{cursoAInscribir.Nombre}'.";
            return RedirectToAction(nameof(Cursos));
        }
    }
}
