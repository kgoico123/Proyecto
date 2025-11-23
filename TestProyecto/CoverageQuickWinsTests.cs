using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto.Models;
using Proyecto.ViewModels;

namespace TestProyecto
{
    [TestClass]
    public class CoverageQuickWinsTests
    {
        [TestMethod]
        public void DocenteCalificacionesVM_Properties_Accessible()
        {
            ValidateProperties(typeof(DocenteCalificacionesVM));
        }

        [TestMethod]
        public void DocenteConductaVM_Properties_Accessible()
        {
            ValidateProperties(typeof(DocenteConductaVM));
        }

        [TestMethod]
        public void NewRegisterTypeUserVM_Properties_Accessible()
        {
            ValidateProperties(typeof(NewRegisterTypeUserVM));
        }

        [TestMethod]
        public void TutorComportamientoVM_Properties_Accessible()
        {
            ValidateProperties(typeof(TutorComportamientoVM));
        }

        [TestMethod]
        public void Calificacion_Model_Properties_Accessible()
        {
            ValidateProperties(typeof(Calificacion));
        }

        private void ValidateProperties(Type t)
        {
            var instance = Activator.CreateInstance(t);
            Assert.IsNotNull(instance);

            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.CanRead && p.CanWrite)
                         .ToArray();

            foreach (var p in props)
            {
                try
                {
                    var sample = GetSampleValue(p.PropertyType);
                    p.SetValue(instance, sample);
                    var read = p.GetValue(instance);
                    // For reference types allow null equality; otherwise assert not null when sample non-null
                    if (sample != null)
                    {
                        Assert.IsNotNull(read, $"Property {t.FullName}.{p.Name} returned null after set");
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed property {t.FullName}.{p.Name}: {ex.Message}");
                }
            }
        }

        private object? GetSampleValue(Type type)
        {
            if (type == typeof(string)) return "x";
            if (type == typeof(int) || type == typeof(int?)) return 1;
            if (type == typeof(long) || type == typeof(long?)) return 1L;
            if (type == typeof(decimal) || type == typeof(decimal?)) return 1m;
            if (type == typeof(double) || type == typeof(double?)) return 1.0;
            if (type == typeof(float) || type == typeof(float?)) return 1f;
            if (type == typeof(bool) || type == typeof(bool?)) return true;
            if (type == typeof(DateTime) || type == typeof(DateTime?)) return DateTime.UtcNow;
            if (type.IsEnum)
            {
                var vals = Enum.GetValues(type);
                return vals.Length > 0 ? vals.GetValue(0) : Activator.CreateInstance(type);
            }
            if (type.IsArray)
            {
                var elem = GetSampleValue(type.GetElementType() ?? typeof(object));
                var arr = Array.CreateInstance(type.GetElementType() ?? typeof(object), 1);
                arr.SetValue(elem, 0);
                return arr;
            }

            if (type.IsGenericType)
            {
                var genDef = type.GetGenericTypeDefinition();
                var arg = type.GetGenericArguments()[0];
                // Handle List<T>, IEnumerable<T>, IList<T>
                if (genDef == typeof(List<>) || genDef == typeof(IEnumerable<>) || genDef == typeof(IList<>))
                {
                    var listType = typeof(List<>).MakeGenericType(arg);
                    var list = Activator.CreateInstance(listType);
                    var add = listType.GetMethod("Add");
                    try
                    {
                        add?.Invoke(list, new[] { GetSampleValue(arg) });
                    }
                    catch
                    {
                        // ignore element add failures
                    }
                    return list;
                }
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var listType = typeof(List<object>);
                return Activator.CreateInstance(listType);
            }

            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }
    }
}
