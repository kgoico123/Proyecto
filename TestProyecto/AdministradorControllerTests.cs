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
using Proyecto;
using Proyecto.shared;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TestProyecto
{
    [TestClass]
    public class AdministradorControllerTests
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
            // Use explicit casts with null-forgiving to avoid CS8625 warnings when constructing UserManager
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
            mgr.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => null as ApplicationUser);
            mgr.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string>());
            mgr.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);
            return mgr;
        }

        private static Mock<UserManager<ApplicationUser>> CreateMockUserManagerWithUser(ApplicationUser user, IList<string> roles)
        {
            var mgr = CreateMockUserManager();
            mgr.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
            mgr.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);
            return mgr;
        }

        [TestMethod]
        public async Task Register_Get_ReturnsViewWithModel()
        {
            // Arrange
            var context = CreateInMemoryContext("reg_get_db");
            // seed cursos and tutores
            context.Cursos.Add(new Curso { IdCurso = 1, Nombre = "Matematica", Grado = "Primero", aula = "A1" });
            context.Tutores.Add(new Tutor { IdTutor = 1, UserId = "seed-user-1" });
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManager();
            var controller = new AdministradorController(userManager.Object, context);

            // Act
            var result = await controller.Register();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(NewRegisterTypeUserVM));
            var model = (NewRegisterTypeUserVM)view.Model;
            Assert.IsTrue(model.cursos != null && model.cursos.Any());
        }

        [TestMethod]
        public async Task Register_Post_InvalidModel_ReturnsViewWithSameModel()
        {
            // Arrange
            var context = CreateInMemoryContext("reg_post_invalid_db");
            var userManager = CreateMockUserManager();
            var controller = new AdministradorController(userManager.Object, context);
            // make modelstate invalid
            controller.ModelState.AddModelError("test", "error");

            var vm = new NewRegisterTypeUserVM();

            // Act
            var result = await controller.Register(vm);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.AreSame(vm, view.Model);
        }

        [TestMethod]
        public async Task RegisterCurso_Get_ReturnsViewWithModelContainingDocentes()
        {
            // Arrange
            var context = CreateInMemoryContext("regcurso_get_db");
            // seed docentes
            context.Docentes.Add(new Docente { IdDocente = 1 });
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManager();
            var controller = new AdministradorController(userManager.Object, context);

            // Act
            var result = await controller.RegisterCurso();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(NewCursoVM));
            var model = (NewCursoVM)view.Model;
            Assert.IsTrue(model.Docentes != null && model.Docentes.Any());
        }
        
        [TestMethod]
        public async Task Register_Post_Valid_Admin_RedirectsToDashboard()
        {
            // Arrange
            var context = CreateInMemoryContext("reg_post_valid_admin_db");
            var user = new ApplicationUser { Id = "u-admin", UserName = "admin1", Dni = "" };
            var userManager = CreateMockUserManagerWithUser(user, new List<string>());
            var controller = new AdministradorController(userManager.Object, context);

            var vm = new NewRegisterTypeUserVM
            {
                tipo = TypesRegister.Administrador,
                User = user
            };

            // Act
            var result = await controller.Register(vm);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual("Dashboard", redirect.ActionName);
        }

        [TestMethod]
        public async Task RegisterCurso_Post_AddsCourseAndRedirects()
        {
            // Arrange
            var context = CreateInMemoryContext("regcurso_post_db");
            var userManager = CreateMockUserManager();
            var controller = new AdministradorController(userManager.Object, context);

            var curso = new Curso { Nombre = "Fisica", Grado = "Segundo", aula = "B2" };
            var vm = new NewCursoVM { Curso = curso, DocenteId = 0 };

            // Act
            var result = await controller.RegisterCurso(vm);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual("Dashboard", redirect.ActionName);
            Assert.IsTrue(context.Cursos.Any(c => c.Nombre == "Fisica"));
        }

        [TestMethod]
        public async Task RegisterCurso_Post_WithDocente_AssignsDocenteCourse()
        {
            // Arrange
            var context = CreateInMemoryContext("regcurso_post_docente_db");
            var docente = new Docente { IdDocente = 1 };
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManager();
            var controller = new AdministradorController(userManager.Object, context);

            var curso = new Curso { Nombre = "Historia", Grado = "Tercero", aula = "C3" };
            var vm = new NewCursoVM { Curso = curso, DocenteId = docente.IdDocente };

            // Act
            var result = await controller.RegisterCurso(vm);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var updatedDocente = context.Docentes.Include(d => d.Curso).FirstOrDefault(d => d.IdDocente == docente.IdDocente);
            Assert.IsNotNull(updatedDocente?.Curso);
            Assert.AreEqual("Historia", updatedDocente.Curso!.Nombre);
        }

        [TestMethod]
        public async Task ActualizarCurso_Post_Valid_UpdatesCourseAndRedirects()
        {
            // Arrange
            var context = CreateInMemoryContext("actualizarcurso_post_db");
            var curso = new Curso { IdCurso = 10, Nombre = "Antiguo", Grado = "Cuarto", aula = "D1" };
            context.Cursos.Add(curso);
            var docente = new Docente { IdDocente = 5 };
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManager();
            var controller = new AdministradorController(userManager.Object, context);

            var updated = new Curso { IdCurso = 10, Nombre = "Nuevo", Grado = "Quinto", aula = "D2" };
            var vm = new NewCursoVM { Curso = updated, DocenteId = docente.IdDocente };

            // Act
            var result = await controller.ActualizarCurso(vm);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var dbCurso = context.Cursos.Find(10);
            Assert.IsNotNull(dbCurso);
            Assert.AreEqual("Nuevo", dbCurso!.Nombre);
            var dbDocente = context.Docentes.Find(docente.IdDocente);
            Assert.AreEqual(dbDocente!.CursoId, dbCurso.IdCurso);
        }

        [TestMethod]
        public async Task EliminarCurso_Post_RemovesCourse_ReturnsJsonSuccess()
        {
            // Arrange
            var context = CreateInMemoryContext("eliminarcurso_post_db");
            var curso = new Curso { IdCurso = 20, Nombre = "ParaEliminar", Grado = "Sexto", aula = "E1" };
            context.Cursos.Add(curso);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManager();
            var controller = new AdministradorController(userManager.Object, context);

            // Act
            var result = await controller.EliminarCurso(curso.IdCurso);

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var json = (JsonResult)result;
            var jsonValue = json.Value!;
            var prop = jsonValue.GetType().GetProperty("success");
            Assert.IsNotNull(prop);
            Assert.IsTrue((bool)prop.GetValue(jsonValue)!);
            Assert.IsFalse(context.Cursos.Any(c => c.IdCurso == curso.IdCurso));
        }

        [TestMethod]
        public async Task DetalleUser_ReturnsNotFound_WhenUserMissing()
        {
            // Arrange
            var context = CreateInMemoryContext("detalleuser_notfound_db");
            var userManager = CreateMockUserManager();
            var controller = new AdministradorController(userManager.Object, context);

            // Act
            var result = await controller.DetalleUser("no-existe");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task DetalleUser_ReturnsView_WhenUserExists()
        {
            // Arrange
            var context = CreateInMemoryContext("detalleuser_ok_db");
            var user = new ApplicationUser { Id = "u1", UserName = "u1" };
            var userManager = CreateMockUserManagerWithUser(user, new List<string> { VCG.Role_Admin });
            var controller = new AdministradorController(userManager.Object, context);

            // Act
            var result = await controller.DetalleUser(user.Id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(UserDetailVM));
        }

        [TestMethod]
        public async Task Usuarios_ReturnsViewWithUsers()
        {
            // Arrange
            var context = CreateInMemoryContext("usuarios_db");
            var user = new ApplicationUser { Id = "u-list", UserName = "lista" };
            // seed into EF so we can return an IAsyncEnumerable-compatible source
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManager();
            userManager.SetupGet(u => u.Users).Returns(context.AppUsers);

            var controller = new AdministradorController(userManager.Object, context);

            // Act
            var result = await controller.Usuarios();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(List<ApplicationUser>));
            var list = (List<ApplicationUser>)view.Model;
            Assert.IsTrue(list.Any(u => u.Id == "u-list"));
        }

        [TestMethod]
        public async Task EliminarUser_Post_RemovesAssociatedEntities_AndRedirects()
        {
            // Arrange
            var context = CreateInMemoryContext("eliminaruser_post_db");
            var user = new ApplicationUser { Id = "del-1", UserName = "deluser" };
            context.Docentes.Add(new Docente { IdDocente = 11, UserId = user.Id });
            context.Estudiantes.Add(new Estudiante { IdEstudiante = 12, UserId = user.Id, Grado = "Primero", TutorId = 0 });
            context.Tutores.Add(new Tutor { IdTutor = 13, UserId = user.Id });
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManagerWithUser(user, new List<string>());
            var controller = new AdministradorController(userManager.Object, context);

            var vm = new UserDetailVM { UserId = user.Id };

            // Act
            var result = await controller.EliminarUser(vm);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual("Usuarios", redirect.ActionName);
            Assert.IsFalse(context.Docentes.Any(d => d.UserId == user.Id));
            Assert.IsFalse(context.Estudiantes.Any(e => e.UserId == user.Id));
            Assert.IsFalse(context.Tutores.Any(t => t.UserId == user.Id));
        }

        [TestMethod]
        public async Task DesvincularDocente_Success_ReturnsJsonSuccess()
        {
            // Arrange
            var context = CreateInMemoryContext("desvincular_docente_db");
            var curso = new Curso { IdCurso = 200, Nombre = "C1", Grado = "G", aula = "A" };
            var docente = new Docente { IdDocente = 50, CursoId = curso.IdCurso, UserId = "u-doc" };
            context.Cursos.Add(curso);
            context.Docentes.Add(docente);
            await context.SaveChangesAsync();

            var userManager = CreateMockUserManager();
            var controller = new AdministradorController(userManager.Object, context);

            var req = new DesvincularDocenteRequest { DocenteId = docente.IdDocente, CursoId = curso.IdCurso };

            // Act
            var result = await controller.DesvincularDocente(req);

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var json = (JsonResult)result;
            var prop = json.Value!.GetType().GetProperty("success");
            Assert.IsNotNull(prop);
            Assert.IsTrue((bool)prop.GetValue(json.Value)!);
            var updated = context.Docentes.Find(docente.IdDocente);
            Assert.IsNull(updated!.CursoId);
        }

        [TestMethod]
        public async Task DetalleUser_ReturnsView_ForTutorEstudianteDocenteRoles()
        {
            // Arrange common
            var context = CreateInMemoryContext("detalle_roles_db");
            var tutorUser = new ApplicationUser { Id = "t1", UserName = "t1" };
            var estudUser = new ApplicationUser { Id = "e1", UserName = "e1" };
            var docUser = new ApplicationUser { Id = "d1", UserName = "d1" };

            context.Tutores.Add(new Tutor { IdTutor = 101, UserId = tutorUser.Id, direccion = "x" });
            context.Estudiantes.Add(new Estudiante { IdEstudiante = 102, UserId = estudUser.Id, Grado = "Primero", TutorId = 0 });
            context.Docentes.Add(new Docente { IdDocente = 103, UserId = docUser.Id });
            await context.SaveChangesAsync();

            // Tutor
            var mgrTutor = CreateMockUserManagerWithUser(tutorUser, new List<string> { VCG.Role_Tutor });
            var ctrlTutor = new AdministradorController(mgrTutor.Object, context);
            var resTutor = await ctrlTutor.DetalleUser(tutorUser.Id);
            Assert.IsInstanceOfType(resTutor, typeof(ViewResult));

            // Estudiante
            var mgrEst = CreateMockUserManagerWithUser(estudUser, new List<string> { VCG.Role_Estudiante });
            var ctrlEst = new AdministradorController(mgrEst.Object, context);
            var resEst = await ctrlEst.DetalleUser(estudUser.Id);
            Assert.IsInstanceOfType(resEst, typeof(ViewResult));

            // Docente
            var mgrDoc = CreateMockUserManagerWithUser(docUser, new List<string> { VCG.Role_Docente });
            var ctrlDoc = new AdministradorController(mgrDoc.Object, context);
            var resDoc = await ctrlDoc.DetalleUser(docUser.Id);
            Assert.IsInstanceOfType(resDoc, typeof(ViewResult));
        }
    }
}
