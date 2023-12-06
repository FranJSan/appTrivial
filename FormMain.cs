using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Trivial
{
    /// todo: eliminar menú de ayuda?
    public partial class FormMain : Form
    {
        public static List<string> configContinentes = new List<string>();
        public static List<string> configPreguntas = new List<string>();
        public static List<string> configRespuestas = new List<string>();


        List<Pais> paises = new List<Pais>();
        Random random = new Random();
        private RoundedButton LabelRespuesta;
        private int puntuacion = 0;

        public FormMain()
        {           
            InitializeComponent();
            CargarPaises("../../capitales.txt");
            CargarConfiguracion();
            GenerarPregunta();

            StartPosition = FormStartPosition.CenterScreen;

            LblRespuesta1.Click += LblRespuesta_Click;
            LblRespuesta2.Click += LblRespuesta_Click;
            LblRespuesta3.Click += LblRespuesta_Click;
            LblRespuesta4.Click += LblRespuesta_Click;
        }

        private void CargarConfiguracion()
        {
            
            if (File.Exists("config.dat"))
            {
                
                List<List<string>> listas = null;
                
                using (Stream stream = new FileStream("config.dat", FileMode.Open, FileAccess.Read))
                {
                    IFormatter formatter = new BinaryFormatter();
                    listas = (List<List<string>>)formatter.Deserialize(stream);
                    stream.Close();
                }

                configContinentes = listas.ElementAt(0);
                configPreguntas = listas.ElementAt(1);
                configRespuestas = listas.ElementAt(2);
                    
            }
        }

        public static void ConfigurarContinentes(List<string> configContinentes)
        {
            FormMain.configContinentes = configContinentes;            
        }
        public static void ConfigurarPreguntas(List<string> configPreguntas)
        {
            FormMain.configPreguntas = configPreguntas;
        }
        public static void ConfigurarRespuestas(List<string> configRespuestas)
        {
            FormMain.configRespuestas = configRespuestas;
        }

        private void LblRespuesta_Click(object sender, EventArgs e)
        {
            RoundedButton btn = (RoundedButton)sender;
            if (btn == LabelRespuesta)
            {
                //MessageBox.Show("¡Bien!");
                btn.AnimacionAcierto.Start();
                btn.AnimacionAcierto.Join();
                GenerarPregunta();
                puntuacion += 3;
                LblPuntuacion.Text = Convert.ToString(puntuacion);
            } else
            {
                //MessageBox.Show("¡Nooo!");
                btn.AnimacionFallo.Start();
                btn.AnimacionFallo.Join();
                //Thread thread = new Thread(btn.AnimarFallo);

                if (puntuacion > 0)
                {
                    puntuacion--;
                    LblPuntuacion.Text = Convert.ToString(puntuacion);
                } 
            }
        }


       
                

        private void AnimarRespuesta(Button btn)
        {
            Color colorOriginal = btn.BackColor;
            btn.BackColor = Color.Red;
            Thread.Sleep(2000);
            btn.BackColor = colorOriginal;

        }

        private void CargarPaises(string path)
        {
            StreamReader sr = new StreamReader(path, Encoding.UTF8);

            string line = sr.ReadLine();

            while (line != null)
            {
                string[] entrada = line.Split(',');
                Pais pais = new Pais(entrada[1], entrada[2], entrada[0]);
                paises.Add(pais);
                line = sr.ReadLine();               
            }

            sr.Close();

        }

        // Proceso de selección de tipo de pregunta:
        //  1 - leer listas de configuración: configContinentes, configPreguntas, configRespuestas
        //  2 - en función a la configuración se deberá actuar de una manera u otra. La configContinentes limita el continente
        //  sobre el que se va a preguntar. Es incompatible con configPreguntas(continentes).
        //  3 - congifPreguntas especifica el tipo de pregunta: sobre la capital, sobre el continente o sobre pais (¿de que país es capital...?
        //  4 - configRepuestas limita las respuestas al mismo continenete. Por ejemplo, si se pregunta, ¿cual es la capital de España?, las opciones de
        //  respuesta serán países de Europa.


        private void GenerarPregunta()
        {
            Pais pais = null;
            // Filtrar por continentes
            if (configContinentes.Count == 0)
            {
                pais = paises.ElementAt(random.Next(0, paises.Count));
            } else
            {   
                do
                {
                    pais = paises.ElementAt(random.Next(0, paises.Count));
                } while (!configContinentes.Contains(pais.Continente));

            }
            // Tipo preguntas
            string tipoP = null;
            string[] tipoPreguntas = { "Continentes", "Paises", "Capitales" };
            if (configPreguntas.Count == 0)
            {
                tipoP = tipoPreguntas[random.Next(0, tipoPreguntas.Length)];
            } else
            {
                tipoP = configPreguntas.ElementAt(random.Next(0, configPreguntas.Count));
            }
            switch (tipoP)
            {
                case "Continentes":
                    if (configPreguntas.Contains("Continentes") && configContinentes.Contains(pais.Continente))
                    {
                        do
                        {
                            pais = paises.ElementAt(random.Next(0, paises.Count));
                        } while (configContinentes.Contains(pais.Continente));
                    }
                    LblPregunta.Text = pais.preguntaContinente();
                    GenerarRespuestas(pais.Continente, "Continentes", pais.Continente);

                    break;
                case "Paises":
                    LblPregunta.Text = pais.preguntaPais();
                    GenerarRespuestas(pais.Nombre, "Paises", pais.Continente);

                    break;
                case "Capitales":
                    LblPregunta.Text = pais.preguntaCapital();
                    GenerarRespuestas(pais.Capital, "Capitales", pais.Continente);
                    break;
                
            }
        }

        private string SeleccionarRespuesta(string tipo, string respuesta)
        {
            if (tipo == null)
            {
                string[] tipos = { "Continentes", "Paises", "Capitales" };
                tipo = tipos[random.Next(0, tipos.Length)];
            }
            Pais pais = null;
            switch (tipo)
            {
                case "Continentes":
                    pais = paises.ElementAt(random.Next(0, paises.Count));
                    while (respuesta.Equals(pais.Continente))
                    {
                        pais = paises.ElementAt(random.Next(0, paises.Count));
                    }

                    return pais.Continente;
                    
                case "Paises":
                    pais = paises.ElementAt(random.Next(0, paises.Count));
                    while (respuesta.Equals(pais.Nombre))
                    {
                        pais = paises.ElementAt(random.Next(0, paises.Count));
                    }

                   return pais.Nombre;
                   
                case "Capitales":
                    pais = paises.ElementAt(random.Next(0, paises.Count));
                    while (respuesta.Equals(pais.Capital))
                    {
                        pais = paises.ElementAt(random.Next(0, paises.Count));
                    }

                    return pais.Capital;
            }

            return null;
        }

        private void GenerarRespuestas(string respuesta, string tipo, string continente)
        {
            LblRespuesta1.Text = "";
            LblRespuesta2.Text = "";
            LblRespuesta3.Text = "";
            LblRespuesta4.Text = "";

            bool mismoCont = false;
            
            if(configRespuestas.Contains("MismoContinente")) 
            {
                mismoCont = true;
            }
            
            
            List<string> listaRespuestas = new List<string>();

            while (listaRespuestas.Count < 3)
            {
                Pais pais = paises.ElementAt(random.Next(0, paises.Count));
                switch (tipo)
                {
                    case "Continentes":
                        if (!respuesta.Equals(pais.Continente) && !listaRespuestas.Contains(pais.Continente))
                        {                            
                            listaRespuestas.Add(pais.Continente);
                            break;
                        }
                        else break; 
                    case "Paises":
                        if (!respuesta.Equals(pais.Nombre) && !listaRespuestas.Contains(pais.Nombre))
                        {
                            if (mismoCont)
                            {
                                if (continente.Equals(pais.Continente))
                                {
                                    listaRespuestas.Add(pais.Nombre);
                                    break;
                                }
                                else break;
                                
                            } else
                            {
                                listaRespuestas.Add(pais.Nombre);
                                break;
                            }
                        }
                        else break;

                    case "Capitales":
                        if (!respuesta.Equals(pais.Capital) && !listaRespuestas.Contains(pais.Capital))
                        {
                            if (mismoCont)
                            {
                                if (continente.Equals(pais.Continente))
                                {
                                    listaRespuestas.Add(pais.Capital);
                                    break;
                                }
                                else break;

                            }
                            else
                            {
                                listaRespuestas.Add(pais.Capital);
                                break;
                            }
                            
                        }
                        else break;                        
                }
            }


            /*


            if (configPreguntas.Count == 0 || configPreguntas.Count == 3)
            {
                string[] tipos = { "Continentes", "Paises", "Capitales" };
                string tipo = tipos[random.Next(0, tipos.Length)];
                for (int i = 0; i < 3; i++)
                {
                    listaRespuestas.Add(SeleccionarRespuesta(tipo, respuesta));
                }
                
            } else 
            {
                
                string tipo = configPreguntas.ElementAt(random.Next(0, configPreguntas.Count));
                for (int i = 0; i < 3; i++)
                {
                    listaRespuestas.Add(SeleccionarRespuesta(tipo, respuesta));
                }
               
            }

            */

            // Esto solo coge respuestas tipo capital
            

            // Mostrar de manera aleatoria las respuestas en los Labels. Para facilitar el algoritmo
            // de selección, creo un número aleatorio del 1 al 4 que indica el LblRespuesta,
            // estableciendo ahí la respuesta correcta. El resto de LblRespuesta se rellena con los
            // datos de la List

            int lblrespuesta = random.Next(1, 5);
            switch (lblrespuesta)
            {
                case 1:
                    LblRespuesta1.Text = respuesta;

                    LblRespuesta2.Text = listaRespuestas.ElementAt(0);
                    LblRespuesta3.Text = listaRespuestas.ElementAt(1);
                    LblRespuesta4.Text = listaRespuestas.ElementAt(2);

                    LabelRespuesta = LblRespuesta1;
                    break;
                case 2:
                    LblRespuesta2.Text = respuesta;

                    LblRespuesta1.Text = listaRespuestas.ElementAt(0);
                    LblRespuesta3.Text = listaRespuestas.ElementAt(1);
                    LblRespuesta4.Text = listaRespuestas.ElementAt(2);

                    LabelRespuesta = LblRespuesta2;
                    break;
                case 3:
                    LblRespuesta3.Text = respuesta;

                    LblRespuesta1.Text = listaRespuestas.ElementAt(0);
                    LblRespuesta2.Text = listaRespuestas.ElementAt(1);
                    LblRespuesta4.Text = listaRespuestas.ElementAt(2);

                    LabelRespuesta = LblRespuesta3;
                    break;
                case 4:
                    LblRespuesta4.Text = respuesta;

                    LblRespuesta1.Text = listaRespuestas.ElementAt(0);
                    LblRespuesta2.Text = listaRespuestas.ElementAt(1);
                    LblRespuesta3.Text = listaRespuestas.ElementAt(2);

                    LabelRespuesta = LblRespuesta4;
                    break;
            }

        }

        private void BtnConf_Click(object sender, EventArgs e)
        {
            FrmConf form = new FrmConf();
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }
    }
}
