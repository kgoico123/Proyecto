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
using System.Linq;

namespace TestProyecto
{
    [TestClass]
    public class TargetedCoverageTests
    {
        private static AppDBContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDBContext(options);
        }

        #pragma warning disable CS8625
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
        #pragma warning restore CS8625

        [TestMethod]
        public async Task Docente_Calificaciones_Get_BadRequest_WhenModelStateInvalid()
        {
            var context = CreateInMemoryContext("tgt_doc_cal_get_bad");
            var user = new ApplicationUser { Id = "d-user", UserName = "doc" };
            context.AppUsers.Add(user);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, userManager.Object);
            controller.ModelState.AddModelError("x","err");

            var result = await controller.Calificaciones();
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Docente_Calificaciones_Post_BadRequest_WhenMissingDataLists()
        {
            var context = CreateInMemoryContext("tgt_doc_cal_post_bad");
            var user = new ApplicationUser { Id = "d2", UserName = "doc2" };
            context.AppUsers.Add(user);
            var curso = new Curso { IdCurso = 1, Nombre = "C1", aula = "A", Grado = "G", HorarioInicio = System.TimeSpan.Zero, HorarioFin = System.TimeSpan.FromHours(1) };
            var docente = new Docente { IdDocente = 11, UserId = user.Id, user = user, Curso = curso };
            context.Cursos.Add(curso);
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, userManager.Object);

            // pass null as the VM to trigger the controller's null checks
            #pragma warning disable CS8625
            var result = await controller.Calificaciones((Proyecto.ViewModels.DocenteCalificacionesVM?)null);
            #pragma warning restore CS8625
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Docente_Conducta_Get_BadRequest_WhenModelStateInvalid()
        {
            var context = CreateInMemoryContext("tgt_doc_cond_get_bad");
            var user = new ApplicationUser { Id = "d3", UserName = "doc3" };
            context.AppUsers.Add(user);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, userManager.Object);
            controller.ModelState.AddModelError("x","err");

            var result = await controller.Conducta();
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Estudiante_Calificaciones_Get_NotFound_WhenNoEstudianteOrNotEnrolled()
        {
            var context = CreateInMemoryContext("tgt_est_cal_get_nf");
            var user = new ApplicationUser { Id = "estx", UserName = "estx" };
            context.AppUsers.Add(user);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new Proyecto.Controllers.EstudianteController(context, userManager.Object);

            // user exists but no Estudiante entity seeded => NotFound
            var result = await controller.Calificaciones(999);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task Estudiante_Calificaciones_Get_ReturnsView_WhenEnrolled()
        {
            var context = CreateInMemoryContext("tgt_est_cal_get_ok");
            var user = new ApplicationUser { Id = "estok", UserName = "estok" };
            context.AppUsers.Add(user);
            var estudiante = new Estudiante { IdEstudiante = 5, UserId = user.Id, Grado = "G", TutorId = 0, user = user };
            context.Estudiantes.Add(estudiante);
            var curso = new Curso { IdCurso = 7, Nombre = "Curso7", Grado = "G", aula = "A", HorarioInicio = System.TimeSpan.Zero, HorarioFin = System.TimeSpan.FromHours(1) };
            context.Cursos.Add(curso);
            var ec = new Estudiante_Curso { IdEstudianteCurso = 8, EstudianteId = estudiante.IdEstudiante, Estudiante = estudiante, CursoId = curso.IdCurso, Curso = curso };
            context.Estudiantes_Cursos.Add(ec);
            var cal = new Calificacion { IdCalificacion = 1, estudiante_CursoId = ec.IdEstudianteCurso, Puntaje = 12, FechaCalificacion = System.DateTime.UtcNow, promedioAcumulado = 12, Comentario = "x" };
            context.Calificaciones.Add(cal);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new Proyecto.Controllers.EstudianteController(context, userManager.Object);

            var result = await controller.Calificaciones(curso.IdCurso);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(Proyecto.ViewModels.EstudianteCalificacionesVM));
        }

        [TestMethod]
        public async Task Tutor_Notificaciones_BadRequest_WhenModelStateInvalid()
        {
            var context = CreateInMemoryContext("tgt_tut_not_bad");
            var user = new ApplicationUser { Id = "tut1", UserName = "tut1" };
            context.AppUsers.Add(user);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new Proyecto.Controllers.TutorController(context, userManager.Object);
            controller.ModelState.AddModelError("x","err");

            var result = await controller.Notificaciones(1);
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
    }
}
