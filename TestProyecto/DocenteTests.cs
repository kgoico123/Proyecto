using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Models;

namespace TestProyecto
{
    [TestClass]
    public class DocenteTests
    {
        [TestMethod]
        public void CrearDocente()
        {
            // Arrange
            var curso = new Curso { IdCurso = 5, Nombre = "Matemática" };
            var usuario = new ApplicationUser { UserName = "docente1", Dni = "12345678", Apellido = "Pérez" };

            var docente = new Docente
            {
                IdDocente = 1,
                UserId = "U100",
                user = usuario,
                CursoId = 5,
                Curso = curso
            };

            // Act & Assert
            Assert.AreEqual(1, docente.IdDocente);
            Assert.AreEqual("U100", docente.UserId);
            Assert.AreSame(usuario, docente.user);
            Assert.AreEqual(5, docente.CursoId);
            Assert.AreSame(curso, docente.Curso);
        }

        [TestMethod]
        public void Docente()
        {
            // Arrange & Act
            var docente = new Docente();

            // Assert
            Assert.AreEqual(0, docente.IdDocente);
            Assert.IsNull(docente.UserId);
            Assert.IsNull(docente.user);
            Assert.IsNull(docente.CursoId);
            Assert.IsNull(docente.Curso);
        }
    }
}
