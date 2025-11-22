using Microsoft.AspNetCore.Mvc;
using Proyecto.Data;
using Proyecto.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Proyecto.shared;
using Proyecto.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Proyecto.Controllers
{
    [Authorize(Roles = VCG.Role_Docente)]
    public class DocenteController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocenteController(AppDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var userName = user.UserName;
            // Removed duplicate null-check for user

            var docenteRes = await _context.Docentes
                .Include(d => d.Curso!)
                    .ThenInclude(c => c.estudiante_Curso!)
                        .ThenInclude(ec => ec.Estudiante!)
                .FirstOrDefaultAsync(d => d.user!.UserName == userName);

            DocenteDashboardVM Dashboard = new DocenteDashboardVM
            {
                docente = docenteRes!,
                curso = docenteRes?.Curso!,
                CantidadAlumnos = docenteRes?.Curso?.estudiante_Curso?.Count() ?? 0
            };

            return View(Dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> Calificaciones(int? horarioId = null, string? seccion = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var userName = user.UserName;
            var docenteRes = await _context.Docentes
                .Include(d => d.Curso!)
                    .ThenInclude(c => c.estudiante_Curso!)
                        .ThenInclude(ec => ec.Estudiante!)
                            .ThenInclude(e => e.user!)
                .FirstOrDefaultAsync(d => d.user!.UserName == userName);

            if (docenteRes == null)
                return Unauthorized();

            // Usar el curso directamente desde el docente
            var curso = docenteRes.Curso;

            // Obtener estudiantes por curso y sección
            var seccionesRes = new List<Secciones>();
            var alumnosCount = 0; // Variable para contar los alumnos
            if (curso != null)
            {
                var alumnos = (curso.estudiante_Curso ?? Enumerable.Empty<Estudiante_Curso>())
                    .Where(ec => ec?.Estudiante != null && ec.Estudiante.user != null)
                    .Select(ec => new AlumnoCalificacionVM
                    {
                        IdEstudiante = ec!.Estudiante!.IdEstudiante,
                        UserId = ec.Estudiante!.UserId ?? string.Empty,
                        Nombre = ec.Estudiante.user!.UserName ?? string.Empty
                    }).ToList();

                alumnosCount = alumnos.Count; // Guardamos la cantidad de alumnos

                seccionesRes.Add(new Secciones
                {
                    Grado = curso?.Nombre ?? string.Empty,
                    Seccion = curso?.aula ?? string.Empty,
                    Alumnos = alumnos
                });
            }

            DocenteCalificacionesVM responseVM = new DocenteCalificacionesVM
            {
                docente = docenteRes!,
                secciones = seccionesRes,
                // Inicializamos las listas para el formulario
                alumnosId = new List<int>(new int[alumnosCount]),
                notas = new List<string>(new string[alumnosCount]),
                comentarios = new List<string>(new string[alumnosCount])
            };

            return View(responseVM);
        }

        [HttpPost]
        public async Task<IActionResult> Calificaciones(DocenteCalificacionesVM data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var userName = user.UserName;

            var docente = await _context.Docentes
                .Include(d => d.Curso!)
                    .ThenInclude(c => c.estudiante_Curso!)
                        .ThenInclude(ec => ec.Estudiante!)
                            .ThenInclude(e => e.user!)
                .FirstOrDefaultAsync(d => d.user!.UserName == userName);

            if (docente == null)
                return Unauthorized();

            if (data?.alumnosId == null || data.notas == null || data.comentarios == null)
                return BadRequest();

            for (int i = 0; i < data.alumnosId.Count; i++)
            {
                int estudianteId = data.alumnosId[i];
                var estudianteCursos = await GetEstudianteCursosAsync(estudianteId, docente.Curso?.IdCurso ?? 0);
                if (estudianteCursos == null || estudianteCursos.Count == 0)
                    continue;

                int nota = MapNota(data.notas.ElementAtOrDefault(i));
                string comentario = data.comentarios.ElementAtOrDefault(i) ?? "Sin comentario";

                foreach (var ec in estudianteCursos)
                {
                    var calificacionesAnteriores = await _context.Calificaciones
                        .Where(c => c.estudiante_CursoId == ec.IdEstudianteCurso)
                        .OrderBy(c => c.FechaCalificacion)
                        .ToListAsync();

                    // Solo permitir máximo 4 registros (bimestres)
                    if (calificacionesAnteriores.Count >= 4)
                        continue;

                    int promedio = CalculatePromedioAcumulado(calificacionesAnteriores, nota);

                    var calificacion = new Calificacion
                    {
                        estudiante_CursoId = ec.IdEstudianteCurso,
                        Puntaje = nota,
                        FechaCalificacion = DateTime.Now,
                        promedioAcumulado = promedio,
                        Comentario = comentario
                    };
                    _context.Calificaciones.Add(calificacion);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard", new { data.seccion });
        }

        private async Task<List<Estudiante_Curso>> GetEstudianteCursosAsync(int estudianteId, int cursoId)
        {
            if (cursoId == 0) return new List<Estudiante_Curso>();

            return await _context.Estudiantes_Cursos
                .Where(ec => ec.EstudianteId == estudianteId && ec.CursoId == cursoId)
                .ToListAsync();
        }

        private int MapNota(string? notaLiteral)
        {
            switch ((notaLiteral ?? string.Empty).Trim().ToUpper())
            {
                case "AD": return 20;
                case "A": return 16;
                case "B": return 12;
                case "C": return 8;
                default: return 0;
            }
        }

        private int CalculatePromedioAcumulado(List<Calificacion> anteriores, int nuevaNota)
        {
            var listaNotas = anteriores.Select(c => c.Puntaje).ToList();
            listaNotas.Add(nuevaNota);
            double promedio = listaNotas.Any() ? listaNotas.Average() : 0;
            if (promedio > 20) promedio = 20;
            return (int)Math.Round(promedio);
        }

        [HttpGet]
        public async Task<IActionResult> Conducta()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var userName = user.UserName;

            var docente = await _context.Docentes
                .Include(d => d.Curso!)
                    .ThenInclude(c => c.estudiante_Curso!)
                        .ThenInclude(ec => ec.Estudiante!)
                            .ThenInclude(e => e.user!)
                .FirstOrDefaultAsync(d => d.user!.UserName == userName);

            var estudiantes = (docente?.Curso?.estudiante_Curso ?? Enumerable.Empty<Estudiante_Curso>())
                                .Where(ec => ec?.Estudiante != null)
                                .Select(ec => ec!.Estudiante!)
                                .ToList();

            DocenteConductaVM conductaVM = new DocenteConductaVM
            {
                Docente = docente,
                Curso = docente?.Curso,
                Estudiantes = estudiantes,
                // Inicializar las listas para que coincidan con el número de estudiantes
                AlumnosId = new List<int>(new int[estudiantes.Count]),
                Conductas = new List<string>(new string[estudiantes.Count]),
                Comentarios = new List<string>(new string[estudiantes.Count])
            };

            return View(conductaVM);
        }

        [HttpPost]
        public async Task<IActionResult> Conducta(DocenteConductaVM dataVm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            for (int i = 0; i < dataVm.AlumnosId.Count; i++)
            {
                var estudianteCurso = await _context.Estudiantes_Cursos.FirstOrDefaultAsync(ec => ec.EstudianteId == dataVm.AlumnosId[i]);
                if (estudianteCurso != null && !string.IsNullOrWhiteSpace(dataVm.Conductas[i]))
                {
                    var comportamiento = new Comportamiento
                    {
                        estudiante_CursoId = estudianteCurso.IdEstudianteCurso,
                        FechaRegistro = DateTime.Now,
                        Calificacion = dataVm.Conductas[i],
                        Descripcion = dataVm.Comentarios[i] ?? ""
                    };
                    _context.Comportamientos.Add(comportamiento);

                    // Si la conducta es "C" o "B", crear notificación al tutor
                    if (dataVm.Conductas[i].Trim().ToUpper() == "C" || dataVm.Conductas[i].Trim().ToUpper() == "B")
                    {
                        // Obtener el estudiante y su tutor
                        var estudiante = await _context.Estudiantes.Include(e => e.user).FirstOrDefaultAsync(e => e.IdEstudiante == dataVm.AlumnosId[i]);
                        if (estudiante != null)
                        {
                            var notificacion = new Notificacion
                            {
                                TutorId = estudiante.TutorId,
                                fecha = DateTime.Now,
                                Titulo = "Alerta de Conducta",
                                Mensaje = $"Se ha registrado una conducta '{dataVm.Conductas[i]}' para el estudiante {estudiante.user?.UserName ?? "Desconocido"}. {dataVm.Comentarios[i]}",
                                Leida = false,
                                Tipo = VCG.TipoNotificacion.advertencia
                            };
                            _context.Notificaciones.Add(notificacion);
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Conducta");
        }
    }
}

