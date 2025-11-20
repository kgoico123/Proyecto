using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Models;
using System;
namespace TestProyecto
{
    [TestClass]
    public class CalificacionTests
    {
        [TestMethod]
        public void CrearCalificacion()
        {
            // Arrange
            var fecha = new DateTime(2025, 10, 4);

            var calificacion = new Calificacion
            {
                IdCalificacion = 1,
                estudiante_CursoId = 10,
                FechaCalificacion = fecha,
                Puntaje = 18,
                Comentario = "Buen desempeño",
                promedioAcumulado = 15
            };

            // Act & Assert
            Assert.AreEqual(1, calificacion.IdCalificacion);
            Assert.AreEqual(10, calificacion.estudiante_CursoId);
            Assert.AreEqual(fecha, calificacion.FechaCalificacion);
            Assert.AreEqual(18, calificacion.Puntaje);
            Assert.AreEqual("Buen desempeño", calificacion.Comentario);
            Assert.AreEqual(15, calificacion.promedioAcumulado);
        }

        [TestMethod]
        public void Calificacion()
        {
            // Arrange & Act
            var calificacion = new Calificacion();

            // Assert: como no se asignó nada, deben ser valores por defecto
            Assert.AreEqual(0, calificacion.IdCalificacion);
            Assert.AreEqual(0, calificacion.estudiante_CursoId);
            Assert.AreEqual(default(DateTime), calificacion.FechaCalificacion);
            Assert.AreEqual(0, calificacion.Puntaje);
            Assert.IsNull(calificacion.Comentario);
            Assert.AreEqual(0, calificacion.promedioAcumulado);
        }
    }
}
