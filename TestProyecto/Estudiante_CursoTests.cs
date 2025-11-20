using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Models;
using System;
using System.Collections.Generic;

namespace TestProyecto
{
    [TestClass]
    public class Estudiante_CursoTests
    {
        [TestMethod]
        public void CrearEstudianteCurso()
        {
            // Arrange
            var fecha = new DateTime(2025, 10, 4);
            var estudiante = new Estudiante { IdEstudiante = 1, Grado = "5to" };
            var curso = new Curso { IdCurso = 2, Nombre = "Matemática" };
            var calificaciones = new List<Calificacion>
            {
                new Calificacion { IdCalificacion = 1, Puntaje = 18 },
                new Calificacion { IdCalificacion = 2, Puntaje = 15 }
            };

            var estudianteCurso = new Estudiante_Curso
            {
                IdEstudianteCurso = 10,
                EstudianteId = 1,
                Estudiante = estudiante,
                CursoId = 2,
                Curso = curso,
                FechaRegistro = fecha,
                Calificaciones = calificaciones
            };

            // Act & Assert
            Assert.AreEqual(10, estudianteCurso.IdEstudianteCurso);
            Assert.AreEqual(1, estudianteCurso.EstudianteId);
            Assert.AreEqual(estudiante, estudianteCurso.Estudiante);
            Assert.AreEqual(2, estudianteCurso.CursoId);
            Assert.AreEqual(curso, estudianteCurso.Curso);
            Assert.AreEqual(fecha, estudianteCurso.FechaRegistro);
            Assert.AreEqual(calificaciones, estudianteCurso.Calificaciones);
        }

        [TestMethod]
        public void EstudianteCurso()
        {
            // Arrange & Act
            var estudianteCurso = new Estudiante_Curso();

            // Assert
            Assert.AreEqual(0, estudianteCurso.IdEstudianteCurso);
            Assert.AreEqual(0, estudianteCurso.EstudianteId);
            Assert.IsNull(estudianteCurso.Estudiante);
            Assert.AreEqual(0, estudianteCurso.CursoId);
            Assert.IsNull(estudianteCurso.Curso);
            Assert.AreEqual(default(DateTime), estudianteCurso.FechaRegistro);
            Assert.IsNull(estudianteCurso.Calificaciones);
        }
    }
}
