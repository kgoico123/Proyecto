using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Models;
using System;
using System.Collections.Generic;

namespace TestProyecto
{
    [TestClass]
    public class CursoTests
    {
        [TestMethod]
        public void CrearCurso()
        {
            // Arrange
            var docentes = new List<Docente>();
            var estudiantesCurso = new List<Estudiante_Curso>();

            var curso = new Curso
            {
                IdCurso = 1,
                Nombre = "Matemática Aplicada",
                HorarioInicio = new TimeSpan(8, 0, 0),
                HorarioFin = new TimeSpan(10, 0, 0),
                aula = "Aula 101",
                Grado = "5to Secundaria",
                Docentes = docentes,
                estudiante_Curso = estudiantesCurso
            };

            // Act & Assert
            Assert.AreEqual(1, curso.IdCurso);
            Assert.AreEqual("Matemática Aplicada", curso.Nombre);
            Assert.AreEqual(new TimeSpan(8, 0, 0), curso.HorarioInicio);
            Assert.AreEqual(new TimeSpan(10, 0, 0), curso.HorarioFin);
            Assert.AreEqual("Aula 101", curso.aula);
            Assert.AreEqual("5to Secundaria", curso.Grado);
            Assert.AreSame(docentes, curso.Docentes);
            Assert.AreSame(estudiantesCurso, curso.estudiante_Curso);
        }

        [TestMethod]
        public void Curso()
        {
            // Arrange & Act
            var curso = new Curso();

            // Assert
            Assert.AreEqual(0, curso.IdCurso);
            Assert.IsNull(curso.Nombre);
            Assert.AreEqual(default(TimeSpan), curso.HorarioInicio);
            Assert.AreEqual(default(TimeSpan), curso.HorarioFin);
            Assert.IsNull(curso.aula);
            Assert.IsNull(curso.Grado);
            Assert.IsNull(curso.Docentes);
            Assert.IsNull(curso.estudiante_Curso);
        }
    }
}
