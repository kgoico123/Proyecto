using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Areas.Identity.Pages.Account.Manage;
using Proyecto.Areas.Identity.Pages.Account;
using Proyecto.ViewModels;
using Proyecto.Models;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http;

namespace TestProyecto
{
    [TestClass]
    public class IdentityAndViewModelTests
    {
        private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mgr = new Mock<UserManager<ApplicationUser>>(store.Object,
                (IOptions<IdentityOptions>)null!, (IPasswordHasher<ApplicationUser>)null!, new IUserValidator<ApplicationUser>[0], new IPasswordValidator<ApplicationUser>[0], (ILookupNormalizer)null!, (IdentityErrorDescriber)null!, (System.IServiceProvider)null!, (ILogger<UserManager<ApplicationUser>>)null!);
            return mgr;
        }

        private static Mock<SignInManager<ApplicationUser>> CreateMockSignInManager(Mock<UserManager<ApplicationUser>> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var mgr = new Mock<SignInManager<ApplicationUser>>(userManager.Object, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
            return mgr;
        }

        [TestMethod]
        public void DocenteCalificacionesVM_Basics()
        {
            var vm = new DocenteCalificacionesVM();
            vm.seccion = "S1";
            vm.alumnosId.Add(1);
            vm.notas.Add("A");
            vm.comentarios.Add("ok");
            Assert.AreEqual("S1", vm.seccion);
            Assert.AreEqual(1, vm.alumnosId.Count);
        }

        [TestMethod]
        public void DocenteConductaVM_Basics()
        {
            var vm = new DocenteConductaVM();
            vm.AlumnosId.Add(2);
            vm.Conductas.Add("C");
            Assert.AreEqual(1, vm.AlumnosId.Count);
            Assert.AreEqual(1, vm.Conductas.Count);
        }

        [TestMethod]
        public void EstudianteCalificacionesVM_Basics()
        {
            var vm = new EstudianteCalificacionesVM();
            vm.NombreCurso = "CursoX";
            vm.Calificaciones.Add(new Calificacion { Puntaje = 10 });
            Assert.AreEqual("CursoX", vm.NombreCurso);
            Assert.AreEqual(1, vm.Calificaciones.Count);
        }

        [TestMethod]
        public void InscripcionCursoVM_Basics()
        {
            var vm = new InscripcionCursoVM();
            vm.CursosDisponibles.Add(new Curso { Nombre = "C1", Grado = "G" });
            Assert.AreEqual(1, vm.CursosDisponibles.Count);
        }

        [TestMethod]
        public async Task DeletePersonalData_OnGet_NotFound_WhenUserMissing()
        {
            var userMgr = CreateMockUserManager();
            userMgr.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null);
            var signIn = CreateMockSignInManager(userMgr);
            var logger = new Mock<ILogger<DeletePersonalDataModel>>();

            var page = new DeletePersonalDataModel(userMgr.Object, signIn.Object, logger.Object);
            var res = await page.OnGet();
            Assert.IsInstanceOfType(res, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task DeletePersonalData_OnPost_PageOnIncorrectPassword()
        {
            var user = new ApplicationUser { Id = "u1" };
            var userMgr = CreateMockUserManager();
            userMgr.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            userMgr.Setup(x => x.HasPasswordAsync(user)).ReturnsAsync(true);
            userMgr.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);
            var signIn = CreateMockSignInManager(userMgr);
            var logger = new Mock<ILogger<DeletePersonalDataModel>>();

            var page = new DeletePersonalDataModel(userMgr.Object, signIn.Object, logger.Object);
            page.Input = new DeletePersonalDataModel.InputModel { Password = "bad" };
            var res = await page.OnPostAsync();
            Assert.IsInstanceOfType(res, typeof(PageResult));
            Assert.IsFalse(page.ModelState.IsValid); // ModelState contains error
        }

        [TestMethod]
        public async Task DeletePersonalData_OnPost_DeletesAndRedirects()
        {
            var user = new ApplicationUser { Id = "u2" };
            var userMgr = CreateMockUserManager();
            userMgr.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            userMgr.Setup(x => x.HasPasswordAsync(user)).ReturnsAsync(false);
            userMgr.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);
            var signIn = CreateMockSignInManager(userMgr);
            signIn.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);
            var logger = new Mock<ILogger<DeletePersonalDataModel>>();

            var page = new DeletePersonalDataModel(userMgr.Object, signIn.Object, logger.Object);
            page.Input = new DeletePersonalDataModel.InputModel { Password = "" };
            var res = await page.OnPostAsync();
            Assert.IsInstanceOfType(res, typeof(RedirectResult));
        }

        [TestMethod]
        public async Task RegisterConfirmation_OnGet_RedirectsWhenEmailNull()
        {
            var userMgr = CreateMockUserManager();
            var page = new RegisterConfirmationModel(userMgr.Object);
            var res = await page.OnGetAsync(null);
            Assert.IsInstanceOfType(res, typeof(RedirectToPageResult));
        }

        [TestMethod]
        public async Task RegisterConfirmation_OnGet_NotFound_WhenUserMissing()
        {
            var userMgr = CreateMockUserManager();
            userMgr.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);
            var page = new RegisterConfirmationModel(userMgr.Object);
            var res = await page.OnGetAsync("missing@example.com", "/");
            Assert.IsInstanceOfType(res, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task Register_OnGet_PopulatesExternalLogins()
        {
            var userMgr = CreateMockUserManager();
            // ensure SupportsUserEmail returns true when called in constructor
            userMgr.SetupGet(x => x.SupportsUserEmail).Returns(true);
            var emailStore = new Mock<IUserEmailStore<ApplicationUser>>();
            var signInMgr = CreateMockSignInManager(userMgr);
            signInMgr.Setup(s => s.GetExternalAuthenticationSchemesAsync()).ReturnsAsync(new List<AuthenticationScheme>());
            var logger = new Mock<ILogger<RegisterModel>>();

            var page = new RegisterModel(userMgr.Object, emailStore.Object, signInMgr.Object, logger.Object);
            await page.OnGetAsync();
            Assert.IsNotNull(page.ExternalLogins);
        }

        [TestMethod]
        public async Task Register_OnPost_InvalidModel_ReturnsPage()
        {
            var userMgr = CreateMockUserManager();
            userMgr.SetupGet(x => x.SupportsUserEmail).Returns(true);
            var emailStore = new Mock<IUserEmailStore<ApplicationUser>>();
            var signInMgr = CreateMockSignInManager(userMgr);
            signInMgr.Setup(s => s.GetExternalAuthenticationSchemesAsync()).ReturnsAsync(new List<AuthenticationScheme>());
            var logger = new Mock<ILogger<RegisterModel>>();

            var page = new RegisterModel(userMgr.Object, emailStore.Object, signInMgr.Object, logger.Object);
            page.Url = Mock.Of<Microsoft.AspNetCore.Mvc.IUrlHelper>(u => u.Content("~/") == "/");
            page.Input = new RegisterModel.InputModel { Username = "u", Email = "e@e.com", Password = "secret", ConfirmPassword = "secret", PhoneNumber = "123", Dni = "d", Apellido = "a" };
            page.ModelState.AddModelError("x", "err");
            var res = await page.OnPostAsync();
            Assert.IsInstanceOfType(res, typeof(PageResult));
        }

        [TestMethod]
        public void TutorViewModels_Basics()
        {
            var t1 = new TutorDashboardVM();
            t1.Estudiantes = new List<Estudiante> { new Estudiante { IdEstudiante = 1, Grado = "G" } };
            var t2 = new TutorCalificacionesVM();
            t2.PromediosPorCurso = new List<TutorCalificacionesVM.PromedioCursoViewModel> { new TutorCalificacionesVM.PromedioCursoViewModel { Curso = "C", Promedio = 5 } };
            var t3 = new TutorComportamientoVM();
            t3.Conductas = new List<Comportamiento> { new Comportamiento { Calificacion = "C" } };
            var t4 = new TutorNotificacionesVM();
            t4.Notificaciones = new List<Notificacion> { new Notificacion { Mensaje = "m", Tipo = Proyecto.shared.VCG.TipoNotificacion.info } };

            Assert.IsTrue(t1.Estudiantes.Any());
            Assert.IsTrue(t2.PromediosPorCurso.Any());
            Assert.IsTrue(t3.Conductas.Any());
            Assert.IsTrue(t4.Notificaciones.Any());
        }
    }
}
