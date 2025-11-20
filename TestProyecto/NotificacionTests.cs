using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Models;
using System;

namespace TestProyecto
{
    [TestClass]
    public class NotificacionTests
    {
        [TestMethod]
        public void CrearNotificacion()
        {
            // Arrange
            var fecha = new DateTime(2025, 10, 4);
            var tutor = new Tutor { IdTutor = 1, direccion = "Av. Principal 123" };

            var notificacion = new Notificacion
            {
                IdNotificacion = 10,
                TutorId = 1,
                Tutor = tutor,
                fecha = fecha,
                Titulo = "Reunión de Padres",
                Mensaje = "La reunión se realizará el lunes a las 9 a.m.",
                Leida = false,
                Tipo = "Aviso"
            };

            // Act & Assert
            Assert.AreEqual(10, notificacion.IdNotificacion);
            Assert.AreEqual(1, notificacion.TutorId);
            Assert.AreEqual(tutor, notificacion.Tutor);
            Assert.AreEqual(fecha, notificacion.fecha);
            Assert.AreEqual("Reunión de Padres", notificacion.Titulo);
            Assert.AreEqual("La reunión se realizará el lunes a las 9 a.m.", notificacion.Mensaje);
            Assert.IsFalse(notificacion.Leida);
            Assert.AreEqual("Aviso", notificacion.Tipo);
        }

        [TestMethod]
        public void Notificacion()
        {
            // Arrange & Act
            var notificacion = new Notificacion();

            // Assert
            Assert.AreEqual(0, notificacion.IdNotificacion);
            Assert.AreEqual(0, notificacion.TutorId);
            Assert.IsNull(notificacion.Tutor);
            Assert.AreEqual(default(DateTime), notificacion.fecha);
            Assert.IsNull(notificacion.Titulo);
            Assert.IsNull(notificacion.Mensaje);
            Assert.IsFalse(notificacion.Leida);
            Assert.IsNull(notificacion.Tipo);
        }
    }
}
