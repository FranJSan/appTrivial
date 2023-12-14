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

/// todo: 
/// eliminar menú de ayuda? de momento no sirve para nada.
/// solucionar perdida del efecto hover en los botones después del cliclk. El problema tiene que estar en las animaciones. 
/// La perdida del efecto hover se produce por la animación. Si no animo y muestro un mensaje informativo el hover se mantiene.
/// al solucionar el problema de la acumulación de Clicks durante la animación, se produjo otro: la animación de acierto dura lo 
/// que debería pero instantaneamente se genera una pregunta nueva. Ocurre por que el hilo realmente ya no se detiene.
/// 
/// 



/// ==============================================================================================================================================
/// =========================================================== NOTAS PARA EL PROFESOR =========================================================== 
/// ==============================================================================================================================================
/// 
/// Hola Javier, te pongo por aquí unas notas:
/// Como te comenté, como el ejercicio era voluntario, solo cogí la idea y fui desarrollando las funcionalidades que se me ocurrian. 
/// Lo que más me está costando es el tema de la animación, sobretodo por la perdida del hover pero tampoco es algo que me preocupe mucho, la verdad. 
/// El resto de funcionalidades implementadas no me han causado mucho problema.
/// Verás que el programa debería mejorarse, no he repasado el código y seguramente se pueda refactorizar y mejorar mucho.
/// El tema de las opciones de configuración tambbién debería mejorarse ya que hay veces que no funciona como me gustaría, pero a nivel estético, 
/// la logica que funciona en la generación de preguntas ha sido correcta en mis pruebas (no sé testing y mucho menos en interfaces gráficas, asique
/// han sido manuales).
/// 
/// Me hubiese gustado meter un Timer y dar la opcion activar una cuenta atrás y bueno más cosas, pero lo dejo para cuando tenga más tiempo además
/// que sino no te mando nunca el proyecto.
/// 
/// 
/// Un saludo y felices fiestas!
/// 




/// Proyecto de práctica sobre preguntas y repuestar de capitales. 
/// El programa genera preguntas sobre capitales, paises y contienentes de manera aleatoria. Cuando se selecciona una
/// respuesta, la aplicación realiza una animación para mostrar al usuario si ha acertado o no. En caso de acierto,
/// se pasa a una nueva pregunta. 
/// Contiene un cuadro informativo sobre la puntiación obtenida y un botón de configuración, donde se podrá configurar el tipo de pregunta, limitar la pregunta a 
/// uno o más continentes concretos o hacer que todas las respuestas sean del mismo continente que la pregunta para que la dificultad
/// sea algo más elevada. La configuración se escribe en el archivo config.dat para que quede guardada de una sesión a otra.
/// 
/// Aún hay que arreglar varias cosas: pérdida del hover en los botones después de animar un fallo/acierto, refactorizar... Pero puede
/// considerarse funcional. Toda la lógica de generación de preguntas funciona bien junto a la configuración del usuario.
/// 
/// La información sobre los paises la he cogido de la web: https://proyectoviajero.com/paises-y-capitales-del-mundo-listado-mapas/
/// Es una tabla excel, a la cual concatené cada registro separando cada celda con una coma para crear un archivo txt y poder procesarlo
/// desde la aplicación. La información de ese listado puede no ser muy exacta. El otro día vi que podría haber trabajado directamente desde
/// el excel como si fuera un base de datos, pero no caí en su momento.


namespace Trivial
{
   
    public partial class FormMain : Form
    {
        // Estas 3 listas estáticas se usan para guardar la configuración del usuario.
        public static List<string> configContinentes = new List<string>();
        public static List<string> configPreguntas = new List<string>();
        public static List<string> configRespuestas = new List<string>();

        // bool para controlar el estado de la animación de los buttomRespuestas
        private static bool animacionEnCurso = false;

        // Lista donde se almacenarán todos los paises del archivo.
        private List<Pais> paises = new List<Pais>();

        // Uso un random amenudo para generar preguntas y respuestas.
        private Random random = new Random();
        
        // Guardo el Button donde se establece la respuesta correcta.
        private RoundedButton BtnRespuesta;

        // Puntuación
        private int puntuacion = 0;


        public FormMain()
        {           
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// En el load se carga todo lo necesario para el funcionamiento del programa: países, configuración, etc
        /// También se estable los manejadores de evento click de los ButtonsRespuesta
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_Load(object sender, EventArgs e)
        {
            CargarPaises("../../capitales.txt");
            CargarConfiguracion();
            GenerarPregunta();

            BtnRespuesta1.Click += BtnRespuesta_Click;
            BtnRespuesta2.Click += BtnRespuesta_Click;
            BtnRespuesta3.Click += BtnRespuesta_Click;
            BtnRespuesta4.Click += BtnRespuesta_Click;
        }

        /// <summary>
        /// Este método carga la configuración, si existe, para que las preguntas y respuestas se ajusten a esta.
        /// </summary>        
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

        /// <summary>
        /// Método static usado desde el FrmConfig para establecer la nueva configuración del usuario.
        /// </summary>
        /// <param name="configContinentes">Nueva lista configuración sobre continentes</param>
        public static void ConfigurarContinentes(List<string> configContinentes)
        {
            FormMain.configContinentes = configContinentes;            
        }

        /// <summary>
        /// Método static usado desde el FrmConfig para guardar la nueva configuración del usuario.
        /// </summary>
        /// <param name="configPreguntas">Nueva lista configuración sobre el tipo de preguntas</param>
        public static void ConfigurarPreguntas(List<string> configPreguntas)
        {
            FormMain.configPreguntas = configPreguntas;
        }

        /// <summary>
        /// Método static usado desde el FrmConfig para establecer la nueva configuración del usuario.
        /// </summary>
        /// <param name="configRespuestas">Nueva lista configuración sobre el tipo de respuestas</param>
        /// <remarks>Sé que unicamente contiene un parámetro, pero decidi usar una lista por si se me ocurría alguna otra opción.</remarks>
        public static void ConfigurarRespuestas(List<string> configRespuestas)
        {
            FormMain.configRespuestas = configRespuestas;
        }

        /// <summary>
        /// Controlador del evento click sobre los Button respuestas. Los 4 tienen este controlador.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Se comprueba si la respuesta selaccionada es la correcta o no, mostran una animación para cada caso.</remarks>
        private void BtnRespuesta_Click(object sender, EventArgs e)
        {
            if (animacionEnCurso) return;
            
            RoundedButton btn = (RoundedButton)sender;
            if (btn == BtnRespuesta)
            {
                animacionEnCurso = true; 
                btn.AnimacionAcierto.Start();
                //btn.AnimacionAcierto.Join(); // Mientras escribía los comentarios sobre el problema, caí en que con un bool ya no me hace falta que espera a terminar 
                                                // la animación y así si puedo gestionar el evento click según el estado del bool.
                animacionEnCurso = false;
                puntuacion += 3;

                GenerarPregunta();
                LblPuntuacion.Text = Convert.ToString(puntuacion);                
            } else
            {
                animacionEnCurso = true;
                btn.AnimacionFallo.Start();                
                animacionEnCurso = false;                

                if (puntuacion > 0)
                {
                    puntuacion--;
                    LblPuntuacion.Text = Convert.ToString(puntuacion);
                } 
            }
        }

        /// <summary>
        /// Método para cargar los países del archivo. El path del archivo se pasa como argumento del método.
        /// </summary>
        /// <param name="path">Path del archivo</param>
        /// <remarks>Se rompe la línea de entrada en las comas, se crea un nuevo País con los datos de esa entradaç
        /// y se guarda en la lista paises</remarks>
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
        //  4 - configRepuestas limita las respuestas al mismo continente. Por ejemplo, si se pregunta, ¿cual es la capital de España?, las opciones de
        //  respuesta serán países de Europa.

        /// <summary>
        /// Método para generar una pregunta aleatoria y mostrarla en el FrmMain.
        /// </summary>
        /// <remarks>Tiene en cuenta la configuración actual del usuario, proporcionando preguntas de acuerdo a esos criterios.
        /// </remarks>
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
            /// Tipo preguntas -> Esta parte fue la que más vueltas he tenido que darle. Un pais contiene tres tipos de preguntas, y se tiene que seleccionar
            /// aquella que cumpla los criterios de la configuración, la cual a veces será un solo tipo, dos o los tres, teniendo que estár generando no solo el 
            /// pais sobre el que se va a preguntar, sino también seleccionar aleatoriamente el tipo de pregunta y proporcionar las respuestar acordes a la pregunta.
            /// 

            string tipoP = null;
            string[] tipoPreguntas = { "Continentes", "Paises", "Capitales" };
            if (configPreguntas.Count == 0) // Si no se ha establecido ningún tipo de pregunta, se selecciona un al azar de entre las 3
            {
                tipoP = tipoPreguntas[random.Next(0, tipoPreguntas.Length)];
            } else // Sino, se selecciona una al azar de entre las que estén en la configuración
            {
                tipoP = configPreguntas.ElementAt(random.Next(0, configPreguntas.Count));
            }
            switch (tipoP) // Una vez decidido el tipo de pregunta, se muestra en pantalla y se ejecuta el método GenerarRespuestas().
            {
                /* En la configuración se puede seleccionar que se pregunte sobre Continentes, lo cual inhabilita la opción de limitar por 
                 * continentes. Pero si además de tipo de pregunta Continentes se selección otro tipo de pregunta, el programa permite al usuario
                 * limitar las preguntas a los continentes que se seleccione. 
                 * Por ello en el caso de continentes, , ya que el filtro por continentes se raliza al principio 
                 * del algoritmo y por lo tanto, si hay filtro, todas las preguntas de tipo continente serás sobre el/los continentes seleccionado.
                 */

                case "Continentes":
                    if (configPreguntas.Contains("Continentes")) 
                    {
                        pais = paises.ElementAt(random.Next(0, paises.Count));                        
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

       
        /// <summary>
        /// Método para generar las respuestas incorrectas apropiadas.
        /// </summary>
        /// <param name="respuesta">string con la respuesta correcta. Se usa para no volver a mostrarla de manera duplicada.</param>
        /// <param name="tipo">Tipo de pregunta (capital, país, continente) para mostrar el mismo tipo de respuestas.</param>
        /// <param name="continente">Cuando está activada en la configuración "Mismo Continente" sobre el tipo de respuesta, era necesario este
        /// parámetro para poder mostrar ese tipo de respuestas.</param>
        /// <remarks>Como hay varios tipos de preguntas, las respuestas tenían que ser las apropiadas. Si se pregunta por una capital
        /// no se puede mostrar como una posible respuesta un pais o un contienente.
        /// 
        /// </remarks>
        private void GenerarRespuestas(string respuesta, string tipo, string continente)
        {
            BtnRespuesta1.Text = "";
            BtnRespuesta2.Text = "";
            BtnRespuesta3.Text = "";
            BtnRespuesta4.Text = "";

            bool mismoCont = false;
            
            if(configRespuestas.Contains("MismoContinente")) 
            {
                mismoCont = true;
            }

            List<string> listaRespuestas = new List<string>();

            while (listaRespuestas.Count < 3) // Genero 3 respuestas en total que se meten en una lista.
            {
                Pais pais = paises.ElementAt(random.Next(0, paises.Count)); // Pais respuesta nuevo
                switch (tipo) // Según el tipo de pregunta, se selecciona una respuesta
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

            MostrarRespuestas(respuesta, listaRespuestas);
        }

        /// <summary>
        /// Método que muestra las respuestas en el Form.
        /// </summary>
        /// <param name="respuesta">respuesta correcta</param>
        /// <param name="listaRespuestas">respuestas incorrectas</param>
        /// <remarks>Posiciona la respuesta correcta en uno de lo Buttons al azar y en el resto las incorrectas.</remarks>
        private void MostrarRespuestas(string respuesta, List<string> listaRespuestas)
        {           
            
            int lblrespuesta = random.Next(1, 5);
            switch (lblrespuesta)
            {
                case 1:
                    BtnRespuesta1.Text = respuesta;

                    BtnRespuesta2.Text = listaRespuestas.ElementAt(0);
                    BtnRespuesta3.Text = listaRespuestas.ElementAt(1);
                    BtnRespuesta4.Text = listaRespuestas.ElementAt(2);

                    BtnRespuesta = BtnRespuesta1;
                    break;
                case 2:
                    BtnRespuesta2.Text = respuesta;

                    BtnRespuesta1.Text = listaRespuestas.ElementAt(0);
                    BtnRespuesta3.Text = listaRespuestas.ElementAt(1);
                    BtnRespuesta4.Text = listaRespuestas.ElementAt(2);

                    BtnRespuesta = BtnRespuesta2;
                    break;
                case 3:
                    BtnRespuesta3.Text = respuesta;

                    BtnRespuesta1.Text = listaRespuestas.ElementAt(0);
                    BtnRespuesta2.Text = listaRespuestas.ElementAt(1);
                    BtnRespuesta4.Text = listaRespuestas.ElementAt(2);

                    BtnRespuesta = BtnRespuesta3;
                    break;
                case 4:
                    BtnRespuesta4.Text = respuesta;

                    BtnRespuesta1.Text = listaRespuestas.ElementAt(0);
                    BtnRespuesta2.Text = listaRespuestas.ElementAt(1);
                    BtnRespuesta3.Text = listaRespuestas.ElementAt(2);

                    BtnRespuesta = BtnRespuesta4;
                    break;
            }
        }

        /// <summary>
        /// Controlador del evento click sobre el botón configuración.
        /// Abre un nuevo FrmConfig para establecer la configuración.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnConf_Click(object sender, EventArgs e)
        {
            FrmConf form = new FrmConf();
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
            GenerarPregunta(); // Al cerrar el Form se genera un nueva pregunta con la nueva configuración.
        }

        public static void SetAnimation()
        {
            animacionEnCurso = !animacionEnCurso;
        }
    }
}
