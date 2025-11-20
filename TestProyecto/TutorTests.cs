using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Models;
using System;
using System.Collections.Generic;

namespace TestProyecto
{
    [TestClass]
    public class TutorTests
    {
        [TestMethod]
        public void CrearTutor()
        {
            // Arrange
            var user = new ApplicationUser { Id = "u123", UserName = "tutor1", Dni = "45678912", Apellido = "García" };

            var estudiante1 = new Estudiante { IdEstudiante = 1, Grado = "5to" };
            var estudiante2 = new Estudiante { IdEstudiante = 2, Grado = "4to" };

            var notificacion = new Notificacion { IdNotificacion = 1, Titulo = "Reunión", Mensaje = "Se realizará el lunes" };

            var tutor = new Tutor
            {
                IdTutor = 10,
                UserId = "u123",
                user = user,
                direccion = "Av. Los Pinos 123",
                Estudiantes = new List<Estudiante> { estudiante1, estudiante2 },
                Notificaciones = new List<Notificacion> { notificacion }
            };

            // Act & Assert
            Assert.AreEqual(10, tutor.IdTutor);
            Assert.AreEqual("u123", tutor.UserId);
            Assert.AreEqual(user, tutor.user);
            Assert.AreEqual("Av. Los Pinos 123", tutor.direccion);

            Assert.AreEqual(2, ((List<Estudiante>)tutor.Estudiantes).Count);
            Assert.AreEqual(1, ((List<Notificacion>)tutor.Notificaciones).Count);
        }

        [TestMethod]
        public void Tutor()
        {
            // Arrange & Act
            var tutor = new Tutor();

            // Assert
            Assert.AreEqual(0, tutor.IdTutor);
            Assert.IsNull(tutor.UserId);
            Assert.IsNull(tutor.user);
            Assert.IsNull(tutor.direccion);
            Assert.IsNull(tutor.Estudiantes);
            Assert.IsNull(tutor.Notificaciones);
        }
    }
}
