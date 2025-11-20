using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto.shared
{
    public class VCG
    {
        public const string Satisfactorio = "Satisfactorio";
        public const string Errado = "Errado";

        // Roles del sistemas
        public const string Role_Admin = "Admin";
        public const string Role_Estudiante = "Estudiante";
        public const string Role_Tutor = "Tutor";
        public const string Role_Docente = "Docente";

        public static class TipoNotificacion
        {
            public const string notas = "Notas";
            public const string advertencia = "Advertencia";
            public const string info = "Informacion";
        }

    }
}
