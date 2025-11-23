using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Areas.Identity.Pages.Account.Manage;
using Proyecto.Models;
using Moq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TestProyecto
{
    [TestClass]
    public class TwoFactorAuthenticationTests
    {
        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
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
            return mgr;
        }

        private static Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(Mock<UserManager<ApplicationUser>> um)
        {
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            contextAccessor.Setup(c => c.HttpContext).Returns(new DefaultHttpContext());
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(o => o.Value).Returns(new IdentityOptions());
            var logger = new Mock<ILogger<SignInManager<ApplicationUser>>>();
            var schemes = new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
            #pragma warning disable CS0618
            var clock = new Mock<Microsoft.AspNetCore.Authentication.ISystemClock>();
            #pragma warning restore CS0618

            var sm = new Mock<SignInManager<ApplicationUser>>(
                um.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                options.Object,
                logger.Object,
                schemes.Object,
                clock.Object
            );
            return sm;
        }

        [TestMethod]
        public async Task OnGetAsync_ReturnsNotFound_WhenUserMissing()
        {
            var um = CreateUserManagerMock();
            var sm = new FakeSignInManager(um.Object) { remembered = false };
            um.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser?)null);
            um.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("no-id");

            var page = new TwoFactorAuthenticationModel(um.Object, sm);
            var result = await page.OnGetAsync();
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task OnGetAsync_SetsPropertiesAndReturnsPage()
        {
            var um = CreateUserManagerMock();
            var sm = new FakeSignInManager(um.Object) { remembered = true };
            var user = new ApplicationUser { Id = "u1" };
            um.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            um.Setup(u => u.GetAuthenticatorKeyAsync(user)).ReturnsAsync("key");
            um.Setup(u => u.GetTwoFactorEnabledAsync(user)).ReturnsAsync(true);
            um.Setup(u => u.CountRecoveryCodesAsync(user)).ReturnsAsync(3);
            // FakeSignInManager will return remembered=true as configured

            var page = new TwoFactorAuthenticationModel(um.Object, sm);
            var result = await page.OnGetAsync();
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsTrue(page.HasAuthenticator);
            Assert.IsTrue(page.Is2faEnabled);
            Assert.IsTrue(page.IsMachineRemembered);
            Assert.AreEqual(3, page.RecoveryCodesLeft);
        }

        [TestMethod]
        public async Task OnPostAsync_ForgetsClientAndRedirects()
        {
            var um = CreateUserManagerMock();
            var sm = new FakeSignInManager(um.Object) { remembered = true };
            var user = new ApplicationUser { Id = "u2" };
            um.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            // FakeSignInManager.ForgetTwoFactorClientAsync is implemented to return CompletedTask

            var page = new TwoFactorAuthenticationModel(um.Object, sm);
            var result = await page.OnPostAsync();
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
            Assert.IsTrue(!string.IsNullOrEmpty(page.StatusMessage));
        }
    }

    // Simple fake that overrides the two methods we need
    internal class FakeSignInManager : SignInManager<ApplicationUser>
    {
        public bool remembered = false;
        public FakeSignInManager(UserManager<ApplicationUser> userManager)
            : base(userManager,
                  new Microsoft.AspNetCore.Http.HttpContextAccessor(),
                  new Moq.Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                  Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
                  new Moq.Mock<Microsoft.Extensions.Logging.ILogger<SignInManager<ApplicationUser>>>().Object,
                  new Moq.Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>().Object,
                  new Moq.Mock<Microsoft.AspNetCore.Identity.IUserConfirmation<ApplicationUser>>().Object)
        {
        }

        public override Task<bool> IsTwoFactorClientRememberedAsync(ApplicationUser user)
        {
            return Task.FromResult(remembered);
        }

        public override Task ForgetTwoFactorClientAsync()
        {
            return Task.CompletedTask;
        }
    }
}
