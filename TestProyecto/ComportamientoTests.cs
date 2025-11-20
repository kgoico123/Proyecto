using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Models;

namespace TestProyecto
{
    [TestClass]
    public class ComportamientoTests
    {
        [TestMethod]
        public void CrearComportamient()
        {
            // Arrange
            var fecha = new DateTime(2025, 10, 4);

            var comportamiento = new Comportamiento
            {
                IdComportamiento = 100,
                estudiante_CursoId = 20,
                FechaRegistro = fecha,
                Calificacion = "Excelente",
                Descripcion = "Participa activamente en clase"
            };

            // Act & Assert
            Assert.AreEqual(100, comportamiento.IdComportamiento);
            Assert.AreEqual(20, comportamiento.estudiante_CursoId);
            Assert.AreEqual(fecha, comportamiento.FechaRegistro);
            Assert.AreEqual("Excelente", comportamiento.Calificacion);
            Assert.AreEqual("Participa activamente en clase", comportamiento.Descripcion);
        }

        [TestMethod]
        public void Comportamiento()
        {
            // Arrange & Act
            var comportamiento = new Comportamiento();

            // Assert: como no se asignó nada, deben ser valores por defecto
            Assert.AreEqual(0, comportamiento.IdComportamiento);
            Assert.AreEqual(0, comportamiento.estudiante_CursoId);
            Assert.AreEqual(default(DateTime), comportamiento.FechaRegistro);
            Assert.IsNull(comportamiento.Calificacion);
            Assert.IsNull(comportamiento.Descripcion);
        }
    }
}
