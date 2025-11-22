using System.Collections.Generic;
using Proyecto.Models;

namespace Proyecto.ViewModels
{
    public class InscripcionCursoVM
    {
        public List<Curso> CursosDisponibles { get; set; } = new List<Curso>();
    }
}
