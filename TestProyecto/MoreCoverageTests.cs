using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Data;
using Proyecto.Models;
using Proyecto.Controllers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TestProyecto
{
    [TestClass]
    public class MoreCoverageTests
    {
        private static AppDBContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDBContext(options);
        }

        private static Mock<UserManager<ApplicationUser>> CreateMockUserManagerWithUser(ApplicationUser user)
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mgr = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                (Microsoft.Extensions.Options.IOptions<IdentityOptions>?)null,
                (IPasswordHasher<ApplicationUser>?)null,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                (ILookupNormalizer?)null,
                (IdentityErrorDescriber?)null,
                (System.IServiceProvider?)null,
                (Microsoft.Extensions.Logging.ILogger<UserManager<ApplicationUser>>?)null
            );
            mgr.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            return mgr;
        }

        [TestMethod]
        public async Task Docente_Calificaciones_Post_Skips_WhenNoEstudianteCursos()
        {
            var context = CreateInMemoryContext("more_doc_cal_skip_empty");
            var user = new ApplicationUser { Id = "doc_skip", UserName = "dskip" };
            context.AppUsers.Add(user);
            var curso = new Curso { IdCurso = 11, Nombre = "Cskip", aula = "A", Grado = "G", HorarioInicio = System.TimeSpan.Zero, HorarioFin = System.TimeSpan.FromHours(1) };
            var docente = new Docente { IdDocente = 21, UserId = user.Id, user = user, Curso = curso };
            context.Cursos.Add(curso);
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, userManager.Object);

            var vm = new Proyecto.ViewModels.DocenteCalificacionesVM
            {
                alumnosId = new List<int> { 999 },
                notas = new List<string> { "A" },
                comentarios = new List<string> { "ok" },
                seccion = "S"
            };

            var result = await controller.Calificaciones(vm);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            // no calificaciones should be added because estudiante_curso missing
            Assert.IsFalse(await context.Calificaciones.AnyAsync());
        }

        [TestMethod]
        public async Task Docente_Calificaciones_Post_Skips_WhenFourExisting()
        {
            var context = CreateInMemoryContext("more_doc_cal_skip_four");
            var user = new ApplicationUser { Id = "doc_four", UserName = "dfour" };
            context.AppUsers.Add(user);
            var curso = new Curso { IdCurso = 12, Nombre = "C4", aula = "A", Grado = "G", HorarioInicio = System.TimeSpan.Zero, HorarioFin = System.TimeSpan.FromHours(1) };
            var estudiante = new Estudiante { IdEstudiante = 50, UserId = "est50", Grado = "G", TutorId = 0, user = new ApplicationUser { Id = "est50u", UserName = "est50" } };
            var ec = new Estudiante_Curso { IdEstudianteCurso = 60, Estudiante = estudiante, EstudianteId = estudiante.IdEstudiante, Curso = curso, CursoId = curso.IdCurso };
            context.Cursos.Add(curso);
            context.Estudiantes.Add(estudiante);
            context.Estudiantes_Cursos.Add(ec);

            // add 4 existing calificaciones
            for (int i = 0; i < 4; i++)
            {
                context.Calificaciones.Add(new Calificacion { estudiante_CursoId = ec.IdEstudianteCurso, Puntaje = 10 + i, FechaCalificacion = System.DateTime.UtcNow.AddDays(-i), promedioAcumulado = 10 + i, Comentario = "x" });
            }

            var docente = new Docente { IdDocente = 31, UserId = user.Id, user = user, Curso = curso };
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, userManager.Object);

            var vm = new Proyecto.ViewModels.DocenteCalificacionesVM
            {
                alumnosId = new List<int> { estudiante.IdEstudiante },
                notas = new List<string> { "A" },
                comentarios = new List<string> { "ok" }
            };

            var result = await controller.Calificaciones(vm);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            // still only 4 existing (new should not be added)
            var count = await context.Calificaciones.CountAsync(c => c.estudiante_CursoId == ec.IdEstudianteCurso);
            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public async Task Docente_Conducta_Post_NoNotification_ForNonCriticalConducta()
        {
            var context = CreateInMemoryContext("more_doc_cond_no_notif");
            var user = new ApplicationUser { Id = "doc_cond", UserName = "dcond" };
            context.AppUsers.Add(user);
            var tutor = new Tutor { IdTutor = 70, UserId = "t1" };
            var estudiante = new Estudiante { IdEstudiante = 71, UserId = "e71", TutorId = tutor.IdTutor, user = new ApplicationUser { Id = "e71u", UserName = "e71" }, Grado = "G" };
            var curso = new Curso { IdCurso = 80, Nombre = "C", Grado = "G", aula = "A", HorarioInicio = System.TimeSpan.Zero, HorarioFin = System.TimeSpan.FromHours(1) };
            var ec = new Estudiante_Curso { IdEstudianteCurso = 81, Estudiante = estudiante, Curso = curso, EstudianteId = estudiante.IdEstudiante, CursoId = curso.IdCurso };
            var docente = new Docente { IdDocente = 41, user = user, UserId = user.Id, Curso = curso };
            context.Tutores.Add(tutor);
            context.Cursos.Add(curso);
            context.Estudiantes.Add(estudiante);
            context.Estudiantes_Cursos.Add(ec);
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, userManager.Object);

            var vm = new Proyecto.ViewModels.DocenteConductaVM
            {
                AlumnosId = new List<int> { estudiante.IdEstudiante },
                Conductas = new List<string> { "A" }, // non-critical
                Comentarios = new List<string> { "ok" }
            };

            var result = await controller.Conducta(vm);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.IsTrue(await context.Comportamientos.AnyAsync(c => c.estudiante_CursoId == ec.IdEstudianteCurso));
            Assert.IsFalse(await context.Notificaciones.AnyAsync());
        }

        [TestMethod]
        public async Task Tutor_LeerNotificacion_ReturnsNotFound_WhenMissing()
        {
            var context = CreateInMemoryContext("more_tut_leer_nf");
            var user = new ApplicationUser { Id = "tut_nf", UserName = "tutnf" };
            context.AppUsers.Add(user);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new TutorController(context, userManager.Object);

            var result = await controller.LeerNotificacion(9999);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
