using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Models;
using System;

namespace TestProyecto
{
    [TestClass]
    public class ApplicationUserTests
    {
        [TestMethod]
        public void CrearApplicationUser()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "juanperez",
                Email = "juan@example.com",
                Dni = "12345678",
                Apellido = "Pérez"
            };

            // Act & Assert
            Assert.AreEqual("user123", user.Id);
            Assert.AreEqual("juanperez", user.UserName);
            Assert.AreEqual("juan@example.com", user.Email);
            Assert.AreEqual("12345678", user.Dni);
            Assert.AreEqual("Pérez", user.Apellido);
        }

        [TestMethod]
        public void ApplicationUser_PermitiraValoresPorDefecto()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.IsNull(user.Id);
            Assert.IsNull(user.UserName);
            Assert.IsNull(user.Email);
            Assert.IsNull(user.Dni);
            Assert.IsNull(user.Apellido);
        }

        [TestMethod]
        public void ApplicationUser_PuedeCambiarValoresDespuesDeCreado()
        {
            // Arrange
            var user = new ApplicationUser();
            user.UserName = "carlos";
            user.Email = "carlos@example.com";
            user.Dni = "76543210";
            user.Apellido = "Ramírez";

            // Act & Assert
            Assert.AreEqual("carlos", user.UserName);
            Assert.AreEqual("carlos@example.com", user.Email);
            Assert.AreEqual("76543210", user.Dni);
            Assert.AreEqual("Ramírez", user.Apellido);
        }
    }
}
