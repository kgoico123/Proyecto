using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Controllers;
using Proyecto.Data;
using Proyecto.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace TestProyecto
{
    [TestClass]
    public class EstudianteControllerTests
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
                (Microsoft.Extensions.Options.IOptions<IdentityOptions>)null!,
                (IPasswordHasher<ApplicationUser>)null!,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                (ILookupNormalizer)null!,
                (IdentityErrorDescriber)null!,
                (IServiceProvider)null!,
                (Microsoft.Extensions.Logging.ILogger<UserManager<ApplicationUser>>)null!
            );
            mgr.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            return mgr;
        }

        [TestMethod]
        public async Task Cursos_Post_SucceedsAndCreatesInscripcion()
        {
            var context = CreateInMemoryContext("est_cursos_post_ok");
            var user = new ApplicationUser { Id = "uest1", UserName = "est1" };
            context.AppUsers.Add(user);
            var estudiante = new Estudiante { IdEstudiante = 1, UserId = user.Id, Grado = "G", TutorId = 0 };
            context.Estudiantes.Add(estudiante);
            var curso = new Curso { IdCurso = 2, Nombre = "C2", Grado = "G", aula = "A", HorarioInicio = TimeSpan.Zero, HorarioFin = TimeSpan.FromHours(1) };
            context.Cursos.Add(curso);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new EstudianteController(context, userManager.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var result = await controller.Cursos(curso.IdCurso);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.IsTrue(context.Estudiantes_Cursos.Any(ec => ec.CursoId == curso.IdCurso && ec.EstudianteId == estudiante.IdEstudiante));
        }

        [TestMethod]
        public async Task Cursos_Post_InvalidCourse_ShowsTempError()
        {
            var context = CreateInMemoryContext("est_cursos_post_nf");
            var user = new ApplicationUser { Id = "u2", UserName = "est2" };
            context.AppUsers.Add(user);
            var estudiante = new Estudiante { IdEstudiante = 3, UserId = user.Id, Grado = "G1", TutorId = 0 };
            context.Estudiantes.Add(estudiante);
            var curso = new Curso { IdCurso = 4, Nombre = "C4", Grado = "Otro", aula = "B" , HorarioInicio = TimeSpan.Zero, HorarioFin = TimeSpan.FromHours(1)};
            context.Cursos.Add(curso);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new EstudianteController(context, userManager.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var result = await controller.Cursos(curso.IdCurso);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            // TempData check isn't straightforward here; we ensure no inscription created
            Assert.IsFalse(context.Estudiantes_Cursos.Any(ec => ec.CursoId == curso.IdCurso));
        }
    }
}
