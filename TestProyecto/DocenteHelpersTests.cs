using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Controllers;
using Proyecto.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace TestProyecto
{
    [TestClass]
    public class DocenteHelpersTests
    {
        [TestMethod]
        public void MapNota_ConvertsLiterals()
        {
            var m = typeof(Proyecto.Controllers.DocenteController).GetMethod("MapNota", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(m);
            Assert.AreEqual(20, m.Invoke(null, new object[] { "AD" }));
            Assert.AreEqual(16, m.Invoke(null, new object[] { "A" }));
            Assert.AreEqual(12, m.Invoke(null, new object[] { "B" }));
            Assert.AreEqual(8, m.Invoke(null, new object[] { "C" }));
            Assert.AreEqual(0, m.Invoke(null, new object[] { "Z" }));
        }

        [TestMethod]
        public void CalculatePromedioAcumulado_ComputesAverageAndCapsAt20()
        {
            var m = typeof(Proyecto.Controllers.DocenteController).GetMethod("CalculatePromedioAcumulado", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(m);

            var list = new List<Proyecto.Models.Calificacion>
            {
                new Proyecto.Models.Calificacion { Puntaje = 18 },
                new Proyecto.Models.Calificacion { Puntaje = 19 }
            };

            var resObj = m.Invoke(null, new object[] { list, 20 });
            Assert.IsNotNull(resObj);
            var res = (int)resObj!;
            Assert.IsTrue(res <= 20);

            var empty = new List<Proyecto.Models.Calificacion>();
            var r2obj = m.Invoke(null, new object[] { empty, 10 });
            Assert.IsNotNull(r2obj);
            var r2 = (int)r2obj!;
            Assert.AreEqual(10, r2);
        }
    }
}
