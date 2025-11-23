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
using System;

namespace TestProyecto
{
    [TestClass]
    public class DocenteControllerTests
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
                (IOptions<IdentityOptions>)null!,
                (IPasswordHasher<ApplicationUser>)null!,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                (ILookupNormalizer)null!,
                (IdentityErrorDescriber)null!,
                (IServiceProvider)null!,
                (ILogger<UserManager<ApplicationUser>>)null!
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
        public async Task Dashboard_ReturnsUnauthorized_WhenNoUserOrDocente()
        {
            var context = CreateInMemoryContext("doc_dash_unauth");
            var userManager = CreateMockUserManager();
            var controller = new DocenteController(context, userManager.Object);

            var result = await controller.Dashboard();
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Dashboard_ReturnsView_WhenDocenteAndCursoExists()
        {
            var context = CreateInMemoryContext("doc_dash_ok");
            var user = new ApplicationUser { Id = "u1", UserName = "doc1" };
            context.AppUsers.Add(user);
            var curso = new Curso { IdCurso = 1, Nombre = "Mat", aula = "A1", Grado = "Primero", HorarioInicio = TimeSpan.Zero, HorarioFin = TimeSpan.FromHours(1) };
            context.Cursos.Add(curso);
            var estudiante = new Estudiante { IdEstudiante = 2, UserId = "eu1", Grado = "G1", TutorId = 0 };
            context.Estudiantes.Add(estudiante);
            var ec = new Estudiante_Curso { IdEstudianteCurso = 3, EstudianteId = estudiante.IdEstudiante, Estudiante = estudiante, CursoId = curso.IdCurso, Curso = curso };
            context.Estudiantes_Cursos.Add(ec);
            var docente = new Docente { IdDocente = 4, UserId = user.Id, user = user, Curso = curso };
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, userManager.Object);

            var result = await controller.Dashboard();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(DocenteDashboardVM));
        }

        [TestMethod]
        public async Task Calificaciones_Get_ReturnsView_WithSecciones()
        {
            var context = CreateInMemoryContext("doc_calificaciones_get");
            var user = new ApplicationUser { Id = "u2", UserName = "doc2" };
            context.AppUsers.Add(user);
            var curso = new Curso { IdCurso = 10, Nombre = "CursoX", aula = "S1", Grado = "G", HorarioInicio = TimeSpan.Zero, HorarioFin = TimeSpan.FromHours(1) };
            context.Cursos.Add(curso);
            var estudiante = new Estudiante { IdEstudiante = 20, UserId = "est1", Grado = "G", TutorId = 0, user = new ApplicationUser { Id = "estuser", UserName = "estu" } };
            context.Estudiantes.Add(estudiante);
            var ec = new Estudiante_Curso { IdEstudianteCurso = 30, EstudianteId = estudiante.IdEstudiante, Estudiante = estudiante, CursoId = curso.IdCurso, Curso = curso };
            context.Estudiantes_Cursos.Add(ec);
            var docente = new Docente { IdDocente = 40, UserId = user.Id, user = user, Curso = curso };
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, userManager.Object);

            var result = await controller.Calificaciones();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(DocenteCalificacionesVM));
            var vm = (DocenteCalificacionesVM)view.Model;
            Assert.IsTrue(vm.secciones != null && vm.secciones.Count > 0);
        }

        [TestMethod]
        public async Task Calificaciones_Post_AddsCalificacion_AndRedirects()
        {
            var context = CreateInMemoryContext("doc_cal_post");
            var user = new ApplicationUser { Id = "u3", UserName = "doc3" };
            context.AppUsers.Add(user);
            var curso = new Curso { IdCurso = 100, Nombre = "C", aula = "A", Grado = "G", HorarioInicio = TimeSpan.Zero, HorarioFin = TimeSpan.FromHours(1) };
            context.Cursos.Add(curso);
            var estudiante = new Estudiante { IdEstudiante = 200, UserId = "e2", Grado = "G", TutorId = 0, user = new ApplicationUser { Id = "est2", UserName = "est2" } };
            context.Estudiantes.Add(estudiante);
            var ec = new Estudiante_Curso { IdEstudianteCurso = 300, EstudianteId = estudiante.IdEstudiante, Estudiante = estudiante, CursoId = curso.IdCurso, Curso = curso };
            context.Estudiantes_Cursos.Add(ec);
            var docente = new Docente { IdDocente = 400, UserId = user.Id, user = user, Curso = curso };
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, userManager.Object);

            var vm = new DocenteCalificacionesVM
            {
                alumnosId = new List<int> { estudiante.IdEstudiante },
                notas = new List<string> { "A" },
                comentarios = new List<string> { "ok" },
                seccion = "S"
            };

            var result = await controller.Calificaciones(vm);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.IsTrue(context.Calificaciones.Any());
        }

        [TestMethod]
        public async Task Conducta_Post_CreatesComportamiento_AndNotification()
        {
            var context = CreateInMemoryContext("doc_cond_post");
            var user = new ApplicationUser { Id = "u4", UserName = "doc4" };
            context.AppUsers.Add(user);
            var tutor = new Tutor { IdTutor = 500, UserId = "tutor1" };
            context.Tutores.Add(tutor);
            var estudiante = new Estudiante { IdEstudiante = 600, UserId = "est3", Grado = "G", TutorId = tutor.IdTutor, user = new ApplicationUser { Id = "est3u", UserName = "est3" } };
            context.Estudiantes.Add(estudiante);
            var curso = new Curso { IdCurso = 700, Nombre = "C2", aula = "A2", Grado = "G", HorarioInicio = TimeSpan.Zero, HorarioFin = TimeSpan.FromHours(1) };
            context.Cursos.Add(curso);
            var ec = new Estudiante_Curso { IdEstudianteCurso = 800, EstudianteId = estudiante.IdEstudiante, Estudiante = estudiante, CursoId = curso.IdCurso, Curso = curso };
            context.Estudiantes_Cursos.Add(ec);
            var docente = new Docente { IdDocente = 900, UserId = user.Id, user = user, Curso = curso };
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user);
            var controller = new DocenteController(context, userManager.Object);

            var vm = new DocenteConductaVM
            {
                AlumnosId = new List<int> { estudiante.IdEstudiante },
                Conductas = new List<string> { "C" },
                Comentarios = new List<string> { "mala" }
            };

            var result = await controller.Conducta(vm);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.IsTrue(context.Comportamientos.Any());
            Assert.IsTrue(context.Notificaciones.Any());
        }
    }
}
