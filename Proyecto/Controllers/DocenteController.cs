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
            var docenteRes = await _context.Docentes
                .Include(d => d.Curso)
                    .ThenInclude(c => c.estudiante_Curso)
                        .ThenInclude(ec => ec.Estudiante)
                .FirstOrDefaultAsync(d => d.user.UserName == user.UserName);

            DocenteDashboardVM Dashboard = new DocenteDashboardVM
            {
                docente = docenteRes,
                curso = docenteRes?.Curso,
                CantidadAlumnos = docenteRes?.Curso?.estudiante_Curso?.Count() ?? 0
            };

            return View(Dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> Calificaciones(int? horarioId = null, string? seccion = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var docenteRes = await _context.Docentes
                .Include(d => d.Curso)
                    .ThenInclude(c => c.estudiante_Curso)
                        .ThenInclude(ec => ec.Estudiante)
                            .ThenInclude(e => e.user)
                .FirstOrDefaultAsync(d => d.user.UserName == user.UserName);

            if (docenteRes == null)
                return Unauthorized();

            // Usar el curso directamente desde el docente
            var curso = docenteRes.Curso;

            // Obtener estudiantes por curso y sección
            var seccionesRes = new List<Secciones>();
            var alumnosCount = 0; // Variable para contar los alumnos
            if (curso != null)
            {
                var alumnos = curso.estudiante_Curso.Select(ec => new AlumnoCalificacionVM
                {
                    IdEstudiante = ec.Estudiante.IdEstudiante,
                    UserId = ec.Estudiante.UserId,
                    Nombre = ec.Estudiante.user.UserName
                }).ToList();

                alumnosCount = alumnos.Count; // Guardamos la cantidad de alumnos

                seccionesRes.Add(new Secciones
                {
                    Grado = curso.Nombre,
                    Seccion = curso.aula,
                    Alumnos = alumnos
                });
            }

            DocenteCalificacionesVM responseVM = new DocenteCalificacionesVM
            {
                docente = docenteRes,
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
            var user = await _userManager.GetUserAsync(User);
            var docente = await _context.Docentes
                .Include(d => d.Curso)
                    .ThenInclude(c => c.estudiante_Curso)
                        .ThenInclude(ec => ec.Estudiante)
                            .ThenInclude(e => e.user)
                .FirstOrDefaultAsync(d => d.user.UserName == user.UserName);
            for (int i = 0; i < data.alumnosId.Count; i++)
            {
                var estudianteCurso = await _context.Estudiantes_Cursos
                    .Where(ec => ec.EstudianteId == data.alumnosId[i] && ec.CursoId == docente.Curso.IdCurso)
                    .ToListAsync();

                foreach (var ec in estudianteCurso)
                {
                    Console.WriteLine($"Estudiante: {ec.EstudianteId}, Curso: {ec.CursoId}, Id: {ec.IdEstudianteCurso}");
                    if (estudianteCurso != null)
                    {
                        // Obtener todas las calificaciones anteriores de este estudiante en este curso
                        var calificacionesAnteriores = await _context.Calificaciones
                            .Where(c => c.estudiante_CursoId == ec.IdEstudianteCurso)
                            .OrderBy(c => c.FechaCalificacion)
                            .ToListAsync();

                        Console.WriteLine($"Calificaciones : {calificacionesAnteriores.Count}");

                        // Solo permitir máximo 4 registros (bimestres)
                        if (calificacionesAnteriores.Count >= 4)
                            continue;

                        // Convertir nota literal a numérica
                        int nota = 0;
                        switch ((data.notas[i] ?? "").Trim().ToUpper())
                        {
                            case "AD":
                                nota = 20;
                                break;
                            case "A":
                                nota = 16;
                                break;
                            case "B":
                                nota = 12;
                                break;
                            case "C":
                                nota = 8;
                                break;
                            default:
                                nota = 0;
                                break;
                        }

                        // Calcular el nuevo promedio acumulado (máximo 20)
                        var listaNotas = calificacionesAnteriores.Select(c => c.Puntaje).ToList();
                        listaNotas.Add(nota);
                        double promedio = listaNotas.Any() ? listaNotas.Average() : 0;
                        if (promedio > 20) promedio = 20;

                        var calificacion = new Calificacion
                        {
                            estudiante_CursoId = ec.IdEstudianteCurso,
                            Puntaje = nota,
                            FechaCalificacion = DateTime.Now,
                            promedioAcumulado = (int)Math.Round(promedio),
                            Comentario = data.comentarios[i] ?? "Sin comentario"
                        };
                        _context.Calificaciones.Add(calificacion);
                    }
                }

            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard", new { data.seccion });
        }

        [HttpGet]
        public async Task<IActionResult> Conducta()
        {
            var user = await _userManager.GetUserAsync(User);
            var docente = await _context.Docentes
                .Include(d => d.Curso)
                    .ThenInclude(c => c.estudiante_Curso)
                        .ThenInclude(ec => ec.Estudiante)
                            .ThenInclude(e => e.user)
                .FirstOrDefaultAsync(d => d.user.UserName == user.UserName);

            var estudiantes = docente?.Curso?.estudiante_Curso
                                .Select(ec => ec.Estudiante)
                                .ToList() ?? new List<Estudiante>();

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

