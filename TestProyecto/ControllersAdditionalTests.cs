using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Controllers;
using Proyecto.Data;
using Proyecto.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace TestProyecto
{
    [TestClass]
    public class ControllersAdditionalTests
    {
        private static AppDBContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDBContext(options);
        }

        private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mgr = new Mock<UserManager<ApplicationUser>>(store.Object,
                (IOptions<IdentityOptions>?)null, (IPasswordHasher<ApplicationUser>?)null, new IUserValidator<ApplicationUser>[0], new IPasswordValidator<ApplicationUser>[0], (ILookupNormalizer?)null, (IdentityErrorDescriber?)null, (System.IServiceProvider?)null, (ILogger<UserManager<ApplicationUser>>?)null);
            mgr.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ClaimsPrincipal cp) => null as ApplicationUser);
            mgr.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            return mgr;
        }

        private static Mock<UserManager<ApplicationUser>> CreateMockUserManagerWithUser(ApplicationUser user, IList<string> roles)
        {
            var mgr = CreateMockUserManager();
            mgr.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mgr.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);
            return mgr;
        }

        [TestMethod]
        public async Task Docente_Dashboard_ReturnsView_WhenDocenteExists()
        {
            var context = CreateInMemoryContext("docente_dash_db");
            var user = new ApplicationUser { Id = "doc1", UserName = "doc1" };
            var curso = new Curso { IdCurso = 100, Nombre = "Mate", Grado = "Primero", aula = "A1" };
            var estudiante = new Estudiante { IdEstudiante = 50, UserId = "est1", user = new ApplicationUser { Id = "est1", UserName = "est1" }, Grado = "Primero" };
            var estudianteCurso = new Estudiante_Curso { IdEstudianteCurso = 500, Estudiante = estudiante, Curso = curso, EstudianteId = estudiante.IdEstudiante, CursoId = curso.IdCurso };
            var docente = new Docente { IdDocente = 10, user = user, Curso = curso };
            curso.estudiante_Curso = new List<Estudiante_Curso> { estudianteCurso };
            context.Cursos.Add(curso);
            context.Estudiantes.Add(estudiante);
            context.Estudiantes_Cursos.Add(estudianteCurso);
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user, new List<string> { "Docente" });
            var controller = new DocenteController(context, userManager.Object);

            var result = await controller.Dashboard();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.IsNotNull(view.Model);
        }

        [TestMethod]
        public async Task Docente_Calificaciones_Post_AddsCalificacion_AndRedirects()
        {
            var context = CreateInMemoryContext("docente_calif_post_db");
            var user = new ApplicationUser { Id = "doc2", UserName = "doc2" };
            var curso = new Curso { IdCurso = 200, Nombre = "Hist", Grado = "Segundo", aula = "B1" };
            var estudiante = new Estudiante { IdEstudiante = 60, UserId = "est2", user = new ApplicationUser { Id = "est2", UserName = "est2" }, Grado = "Segundo" };
            var estudianteCurso = new Estudiante_Curso { IdEstudianteCurso = 600, Estudiante = estudiante, Curso = curso, EstudianteId = estudiante.IdEstudiante, CursoId = curso.IdCurso };
            var docente = new Docente { IdDocente = 20, user = user, Curso = curso };
            curso.estudiante_Curso = new List<Estudiante_Curso> { estudianteCurso };
            context.Cursos.Add(curso);
            context.Estudiantes.Add(estudiante);
            context.Estudiantes_Cursos.Add(estudianteCurso);
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user, new List<string> { "Docente" });
            var controller = new DocenteController(context, userManager.Object);

            var vm = new Proyecto.ViewModels.DocenteCalificacionesVM
            {
                alumnosId = new List<int> { estudiante.IdEstudiante },
                notas = new List<string> { "A" },
                comentarios = new List<string> { "Bien" }
            };

            var result = await controller.Calificaciones(vm);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.IsTrue(await context.Calificaciones.AnyAsync());
        }

        [TestMethod]
        public async Task Docente_Conducta_Post_CreatesComportamiento_AndNotificacion()
        {
            var context = CreateInMemoryContext("docente_cond_post_db");
            var user = new ApplicationUser { Id = "doc3", UserName = "doc3" };
            var tutor = new Tutor { IdTutor = 7, UserId = "tut1" };
            var estudiante = new Estudiante { IdEstudiante = 70, UserId = "est3", TutorId = tutor.IdTutor, user = new ApplicationUser { Id = "est3", UserName = "est3" }, Grado = "Tercero" };
            var curso = new Curso { IdCurso = 300, Nombre = "Geo", Grado = "Tercero", aula = "C1" };
            var estudianteCurso = new Estudiante_Curso { IdEstudianteCurso = 700, Estudiante = estudiante, Curso = curso, EstudianteId = estudiante.IdEstudiante, CursoId = curso.IdCurso };
            var docente = new Docente { IdDocente = 30, user = user, Curso = curso };
            curso.estudiante_Curso = new List<Estudiante_Curso> { estudianteCurso };
            context.Tutores.Add(tutor);
            context.Cursos.Add(curso);
            context.Estudiantes.Add(estudiante);
            context.Estudiantes_Cursos.Add(estudianteCurso);
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user, new List<string> { "Docente" });
            var controller = new DocenteController(context, userManager.Object);

            var vm = new Proyecto.ViewModels.DocenteConductaVM
            {
                AlumnosId = new List<int> { estudiante.IdEstudiante },
                Conductas = new List<string> { "C" },
                Comentarios = new List<string> { "Incidente" }
            };

            var result = await controller.Conducta(vm);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.IsTrue(await context.Comportamientos.AnyAsync());
            Assert.IsTrue(await context.Notificaciones.AnyAsync());
        }

        [TestMethod]
        public async Task Estudiante_Dashboard_ReturnsView_WithCourses()
        {
            var context = CreateInMemoryContext("est_dash_db");
            var user = new ApplicationUser { Id = "estuser", UserName = "estuser" };
            var curso = new Curso { IdCurso = 400, Nombre = "Bio", Grado = "Cuarto", aula = "D1" };
            var estudiante = new Estudiante { IdEstudiante = 80, UserId = user.Id, user = user, Grado = "Cuarto" };
            var estudianteCurso = new Estudiante_Curso { IdEstudianteCurso = 800, Estudiante = estudiante, Curso = curso, EstudianteId = estudiante.IdEstudiante, CursoId = curso.IdCurso };
            curso.estudiante_Curso = new List<Estudiante_Curso> { estudianteCurso };
            context.Cursos.Add(curso);
            context.Estudiantes.Add(estudiante);
            context.Estudiantes_Cursos.Add(estudianteCurso);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user, new List<string> { "Estudiante" });
            var controller = new EstudianteController(context, userManager.Object);

            var result = await controller.Dashboard();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(Proyecto.ViewModels.EstudianteDashboardVM));
        }

        [TestMethod]
        public async Task Tutor_Dashboard_ReturnsView_WithStudents()
        {
            var context = CreateInMemoryContext("tutor_dash_db");
            var user = new ApplicationUser { Id = "tutuser", UserName = "tutuser" };
            var tutor = new Tutor { IdTutor = 90, UserId = user.Id };
            var estudiante = new Estudiante { IdEstudiante = 91, UserId = "e91", TutorId = tutor.IdTutor, user = new ApplicationUser { Id = "e91", UserName = "e91" }, Grado = "Quinto" };
            tutor.Estudiantes = new List<Estudiante> { estudiante };
            context.Tutores.Add(tutor);
            context.Estudiantes.Add(estudiante);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user, new List<string> { "Tutor" });
            var controller = new TutorController(context, userManager.Object);

            var result = await controller.Dashboard();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(Proyecto.ViewModels.TutorDashboardVM));
        }

        [TestMethod]
        public async Task Tutor_LeerNotificacion_MarksAsRead_AndReturnsView()
        {
            var context = CreateInMemoryContext("tutor_leer_not_db");
            var user = new ApplicationUser { Id = "tut2", UserName = "tut2" };
            var noti = new Notificacion { IdNotificacion = 1000, TutorId = 1, Leida = false, Mensaje = "msg", Titulo = "t", Tipo = Proyecto.shared.VCG.TipoNotificacion.advertencia };
            context.Notificaciones.Add(noti);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user, new List<string> { "Tutor" });
            var controller = new TutorController(context, userManager.Object);

            var result = await controller.LeerNotificacion(noti.IdNotificacion, "http://r");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var n = await context.Notificaciones.FindAsync(noti.IdNotificacion);
            Assert.IsTrue(n!.Leida);
        }
    }
}
