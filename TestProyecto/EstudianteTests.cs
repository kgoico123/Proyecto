using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Models;
using System;
using System.Collections.Generic;

namespace TestProyecto
{
    [TestClass]
    public class EstudianteTests
    {
        [TestMethod]
        public void CrearEstudiante()
        {
            // Arrange
            var user = new ApplicationUser { Id = "U123", UserName = "juanperez" };
            var tutor = new Tutor { IdTutor = 5, direccion = "Av. Lima 123" };

            var estudianteCursos = new List<Estudiante_Curso>
            {
                new Estudiante_Curso { IdEstudianteCurso = 1, CursoId = 2, EstudianteId = 10 },
                new Estudiante_Curso { IdEstudianteCurso = 2, CursoId = 3, EstudianteId = 10 }
            };

            var estudiante = new Estudiante
            {
                IdEstudiante = 10,
                UserId = "U123",
                user = user,
                TutorId = 5,
                Tutor = tutor,
                Grado = "5to Secundaria",
                Estudiante_Cursos = estudianteCursos
            };

            // Act & Assert
            Assert.AreEqual(10, estudiante.IdEstudiante);
            Assert.AreEqual("U123", estudiante.UserId);
            Assert.AreEqual(user, estudiante.user);
            Assert.AreEqual(5, estudiante.TutorId);
            Assert.AreEqual(tutor, estudiante.Tutor);
            Assert.AreEqual("5to Secundaria", estudiante.Grado);
            Assert.AreEqual(estudianteCursos, estudiante.Estudiante_Cursos);
            Assert.AreEqual(2, ((List<Estudiante_Curso>)estudiante.Estudiante_Cursos).Count);
        }

        [TestMethod]
        public void Estudiante()
        {
            // Arrange & Act
            var estudiante = new Estudiante();

            // Assert
            Assert.AreEqual(0, estudiante.IdEstudiante);
            Assert.IsNull(estudiante.UserId);
            Assert.IsNull(estudiante.user);
            Assert.AreEqual(0, estudiante.TutorId);
            Assert.IsNull(estudiante.Tutor);
            Assert.IsNull(estudiante.Grado);
            Assert.IsNull(estudiante.Estudiante_Cursos);
        }
    }
}
