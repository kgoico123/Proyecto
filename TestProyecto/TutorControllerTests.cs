using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Controllers;
using Proyecto.Data;
using Proyecto.Models;
using Proyecto.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TestProyecto
{
    [TestClass]
    public class TutorControllerTests
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
            var mgr = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                (IOptions<IdentityOptions>?)null,
                (IPasswordHasher<ApplicationUser>?)null,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                (ILookupNormalizer?)null,
                (IdentityErrorDescriber?)null,
                (IServiceProvider?)null,
                (ILogger<UserManager<ApplicationUser>>?)null
            );
            mgr.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync((ApplicationUser?)null);
            return mgr;
        }

        private static Mock<UserManager<ApplicationUser>> CreateMockUserManagerWithUser(ApplicationUser user)
        {
            var mgr = CreateMockUserManager();
            mgr.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            return mgr;
        }

        [TestMethod]
        public async Task Dashboard_ReturnsUnauthorized_WhenNoUser()
        {
            var context = CreateInMemoryContext("tut_dash_unauth");
            var userManager = CreateMockUserManager();
            var controller = new TutorController(context, userManager.Object);

            var result = await controller.Dashboard();
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Dashboard_ReturnsView_WithEstudiantes()
        {
            var context = CreateInMemoryContext("tut_dash_ok");
            var user = new ApplicationUser { Id = "tu1", UserName = "tu1" };
            context.AppUsers.Add(user);
            var estudiante = new Estudiante { IdEstudiante = 10, UserId = "e10", Grado = "G", TutorId = 1, user = new ApplicationUser { Id = "est10", UserName = "est10" } };
            var tutor = new Tutor { IdTutor = 1, UserId = user.Id, Estudiantes = new List<Estudiante> { estudiante } };
            context.Tutores.Add(tutor);
            context.Estudiantes.Add(estudiante);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new TutorController(context, userManager.Object);

            var result = await controller.Dashboard();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(TutorDashboardVM));
        }

        [TestMethod]
        public async Task Notificaciones_ReturnsNotFound_WhenEstudianteMissing()
        {
            var context = CreateInMemoryContext("tut_notif_nf");
            var userManager = CreateMockUserManager();
            var controller = new TutorController(context, userManager.Object);

            var result = await controller.Notificaciones(9999);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task LeerNotificacion_MarksRead_AndReturnsView()
        {
            var context = CreateInMemoryContext("tut_leer_not");
            var noti = new Notificacion { IdNotificacion = 1, TutorId = 2, Leida = false, fecha = System.DateTime.UtcNow, Titulo = "T", Mensaje = "M", Tipo = Proyecto.shared.VCG.TipoNotificacion.info };
            context.Notificaciones.Add(noti);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManager();
            var controller = new TutorController(context, userManager.Object);

            var result = await controller.LeerNotificacion(noti.IdNotificacion, "http://localhost/");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var updated = context.Notificaciones.Find(noti.IdNotificacion);
            Assert.IsTrue(updated!.Leida);
        }

        [TestMethod]
        public async Task Calificaciones_ReturnsView_WithPromedios()
        {
            var context = CreateInMemoryContext("tut_califs");
            var estudiante = new Estudiante { IdEstudiante = 20, UserId = "e20", Grado = "G", TutorId = 3, user = new ApplicationUser { Id = "est20", UserName = "est20" } };
            context.Estudiantes.Add(estudiante);
            var curso = new Curso { IdCurso = 30, Nombre = "Cur", aula = "A", Grado = "G", HorarioInicio = TimeSpan.Zero, HorarioFin = TimeSpan.FromHours(1) };
            context.Cursos.Add(curso);
            var ec = new Estudiante_Curso { IdEstudianteCurso = 40, EstudianteId = estudiante.IdEstudiante, Estudiante = estudiante, CursoId = curso.IdCurso, Curso = curso };
            context.Estudiantes_Cursos.Add(ec);
            var cal = new Calificacion { IdCalificacion = 1, estudiante_CursoId = ec.IdEstudianteCurso, Puntaje = 15, promedioAcumulado = 15, FechaCalificacion = System.DateTime.UtcNow, Comentario = "x" };
            context.Calificaciones.Add(cal);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManager();
            var controller = new TutorController(context, userManager.Object);

            var result = await controller.Calificaciones(estudiante.IdEstudiante);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(TutorCalificacionesVM));
            var vm = (TutorCalificacionesVM)view.Model;
            Assert.IsTrue(vm.Calificaciones.Any());
        }
    }
}
