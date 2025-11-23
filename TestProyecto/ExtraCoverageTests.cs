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
    public class ExtraCoverageTests
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
        public async Task Docente_Calificaciones_Get_DocenteWithoutCourse_ReturnsView_EmptySecciones()
        {
            var context = CreateInMemoryContext("extra_doc_get_no_course");
            var user = new ApplicationUser { Id = "dnc", UserName = "dnc" };
            context.AppUsers.Add(user);
            var docente = new Docente { IdDocente = 500, UserId = user.Id, user = user, Curso = null };
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var um = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, um.Object);

            var result = await controller.Calificaciones();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            var vm = view.Model as Proyecto.ViewModels.DocenteCalificacionesVM;
            Assert.IsNotNull(vm);
            Assert.IsTrue(vm!.secciones != null && vm.secciones.Count == 0);
        }

        [TestMethod]
        public async Task Docente_Calificaciones_Post_UnknownNota_MapsToZero()
        {
            var context = CreateInMemoryContext("extra_doc_post_map_zero");
            var user = new ApplicationUser { Id = "dmap", UserName = "dmap" };
            context.AppUsers.Add(user);
            var curso = new Curso { IdCurso = 210, Nombre = "Cmap", aula = "A", Grado = "G", HorarioInicio = System.TimeSpan.Zero, HorarioFin = System.TimeSpan.FromHours(1) };
            var estudiante = new Estudiante { IdEstudiante = 211, UserId = "e211", Grado = "G", TutorId = 0, user = new ApplicationUser { Id = "e211u", UserName = "e211" } };
            var ec = new Estudiante_Curso { IdEstudianteCurso = 212, Estudiante = estudiante, EstudianteId = estudiante.IdEstudiante, Curso = curso, CursoId = curso.IdCurso };
            var docente = new Docente { IdDocente = 213, UserId = user.Id, user = user, Curso = curso };
            context.Cursos.Add(curso);
            context.Estudiantes.Add(estudiante);
            context.Estudiantes_Cursos.Add(ec);
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var um = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, um.Object);

            var vm = new Proyecto.ViewModels.DocenteCalificacionesVM
            {
                alumnosId = new List<int> { estudiante.IdEstudiante },
                notas = new List<string> { "Z" }, // unknown -> 0
                comentarios = new List<string> { "c" }
            };

            var result = await controller.Calificaciones(vm);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var created = await context.Calificaciones.FirstOrDefaultAsync();
            Assert.IsNotNull(created);
            Assert.AreEqual(0, created!.Puntaje);
        }

        [TestMethod]
        public async Task Docente_Conducta_Post_Ignores_EmptyConducta()
        {
            var context = CreateInMemoryContext("extra_doc_cond_ignore_empty");
            var user = new ApplicationUser { Id = "dce", UserName = "dce" };
            context.AppUsers.Add(user);
            var estudiante = new Estudiante { IdEstudiante = 301, UserId = "e301", TutorId = 0, user = new ApplicationUser { Id = "e301u", UserName = "e301" }, Grado = "G" };
            var curso = new Curso { IdCurso = 302, Nombre = "C302", Grado = "G", aula = "A", HorarioInicio = System.TimeSpan.Zero, HorarioFin = System.TimeSpan.FromHours(1) };
            var ec = new Estudiante_Curso { IdEstudianteCurso = 303, Estudiante = estudiante, Curso = curso, EstudianteId = estudiante.IdEstudiante, CursoId = curso.IdCurso };
            var docente = new Docente { IdDocente = 304, UserId = user.Id, user = user, Curso = curso };
            context.Cursos.Add(curso);
            context.Estudiantes.Add(estudiante);
            context.Estudiantes_Cursos.Add(ec);
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var um = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, um.Object);

            var vm = new Proyecto.ViewModels.DocenteConductaVM
            {
                AlumnosId = new List<int> { estudiante.IdEstudiante },
                Conductas = new List<string> { " " },
                Comentarios = new List<string> { "x" }
            };

            var result = await controller.Conducta(vm);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.IsFalse(await context.Comportamientos.AnyAsync());
        }

        [TestMethod]
        public async Task Tutor_Notificaciones_ReturnsView_WithNotifications()
        {
            var context = CreateInMemoryContext("extra_tut_notifs_ok");
            var tutor = new Tutor { IdTutor = 401, UserId = "t401" };
            var estudiante = new Estudiante { IdEstudiante = 402, UserId = "e402", TutorId = tutor.IdTutor, user = new ApplicationUser { Id = "e402u", UserName = "e402" }, Grado = "G" };
            var noti = new Notificacion { IdNotificacion = 500, TutorId = tutor.IdTutor, Leida = false, fecha = System.DateTime.UtcNow, Titulo = "T", Mensaje = "M", Tipo = Proyecto.shared.VCG.TipoNotificacion.info };
            context.Tutores.Add(tutor);
            context.Estudiantes.Add(estudiante);
            context.Notificaciones.Add(noti);
            await context.SaveChangesAsync();

            var user = new ApplicationUser { Id = "tuser", UserName = "tuser" };
            context.AppUsers.Add(user);
            await context.SaveChangesAsync();

            var um = CreateMockUserManagerWithUser(user);
            var controller = new TutorController(context, um.Object);

            // call Notificaciones with estudiante id that exists
            var result = await controller.Notificaciones(estudiante.IdEstudiante);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            var vm = view.Model as Proyecto.ViewModels.TutorNotificacionesVM;
            Assert.IsNotNull(vm);
        }
    }
}
