using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial
{
    class Pais
    {
        public string Nombre { get; set; }
        public string Capital { get; set; }
        public string Continente { get; set; }

        public Pais(string nombre, string capital, string continente)
        {
            Nombre = nombre;
            Capital = capital;
            Continente = continente;
        }

        public string preguntaCapital()
        {
            return "¿Cual es la capital de\n " + Nombre + " ?";
        }

        public string preguntaPais()
        {
            return "¿De que país es capital\n " + Capital + " ?";
        }

        public string preguntaContinente()
        {
            return "¿En qué continente se encuentra\n " + Nombre + " ?";
        }

       
    }
}
