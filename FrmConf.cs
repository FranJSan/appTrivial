﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Trivial
{
    /// <summary>
    /// Este Form permite al usuario establecer la configuración de las preguntas:
    ///     1. Permite establecer sobre el continente en el que versarán las preguntas.
    ///     2. Permite establecer el tipo de pregunta: 
    ///         - ¿Cual es la capital de...?
    ///         - ¿De qué país es capital...?
    ///         - ¿En qué continente se encuentra...?
    /// </summary>
    public partial class FrmConf : Form
    {
        
        public FrmConf()
        {
            InitializeComponent();
            
        }
        private void FrmConf_Load(object sender, EventArgs e)
        {

            ConfigCBContinentes();
            CargarConfiguracion();
        }
        private void ConfigCBContinentes()
        {
            foreach (CheckBox cb in PnlContinentes.Controls.OfType<CheckBox>())
            {
                cb.CheckedChanged += new EventHandler(ChangedCBContinentes);
            }
            foreach (CheckBox cb in PnlPreguntas.Controls.OfType<CheckBox>())
            {
                cb.CheckedChanged += new EventHandler(ChangedCBPreguntas);
            }
            foreach (CheckBox cb in PnlResp.Controls.OfType<CheckBox>())
            {
                cb.CheckedChanged += new EventHandler(ChangedCBRespuestas);
            }
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

                List<string> configContinentes = listas.ElementAt(0);
                List<string> configPreguntas = listas.ElementAt(1);
                List<string> configRespuestas = listas.ElementAt(2);

                foreach(CheckBox cb in PnlContinentes.Controls.OfType<CheckBox>())
                {
                    if (configContinentes.Contains((string)cb.Tag)) cb.Checked = true;
                }
                foreach (CheckBox cb in PnlPreguntas.Controls.OfType<CheckBox>())
                {
                    if (configPreguntas.Contains((string)cb.Tag)) cb.Checked = true;
                }
                foreach (CheckBox cb in PnlResp.Controls.OfType<CheckBox>())
                {
                    if (configRespuestas.Contains((string)cb.Tag)) cb.Checked = true;
                }

            }
        }

        public void ChangedCBContinentes(Object sender, EventArgs e)
        {
            bool check = false;
            foreach (CheckBox cb in PnlContinentes.Controls.OfType<CheckBox>())
            {
                if (cb.Checked)
                {
                    check = true;
                    break;
                }
                else check = false;
            }

            if (check)
            {
                CBContiTPregunta.Enabled = false;
            }
            else CBContiTPregunta.Enabled = true;
        }
        public void ChangedCBPreguntas(Object sender, EventArgs e)
        {
            int totalCheck = 0;
            foreach (CheckBox cb in PnlPreguntas.Controls.OfType<CheckBox>())
            {
                if (cb.Checked) totalCheck++;
            }

            if (CBContiTPregunta.Checked && totalCheck == 1)
            {
                foreach (CheckBox cb in PnlContinentes.Controls.OfType<CheckBox>())
                {                    
                    cb.Enabled = false;
                }

                CBMismoCont.Enabled = false;
            } else
            {
                foreach (CheckBox cb in PnlContinentes.Controls.OfType<CheckBox>())
                {
                    cb.Enabled = true;
                }

                CBMismoCont.Enabled = true;
            }
        }
        public void ChangedCBRespuestas(Object sender, EventArgs e)
        {
            if (CBMismoCont.Checked)
            {
                

                CBContiTPregunta.Enabled = false;
            }
            else
            {
                

                CBContiTPregunta.Enabled = true;
            }
        }
        private void BtnGuardarConf_Click(object sender, EventArgs e)
        {
            List<string> listContinentes = new List<string>();
            foreach (CheckBox temp in PnlContinentes.Controls.OfType<CheckBox>())
            {
                if (temp.Enabled && temp.Checked)
                {
                    listContinentes.Add((string)temp.Tag);
                }
            }

            List<string> listPreguntas = new List<string>();
            int total = 0;
            foreach (CheckBox temp in PnlPreguntas.Controls.OfType<CheckBox>())
            {
                if (CBContiTPregunta.Checked) total++;
            }

            foreach (CheckBox temp in PnlPreguntas.Controls.OfType<CheckBox>())
            {
                if (temp.Checked)
                {
                    if (temp == CBContiTPregunta)
                    {
                        if (total > 0)
                        {
                            listPreguntas.Add((string)temp.Tag);                            
                        }
                    } else listPreguntas.Add((string)temp.Tag);
                    
                }
            }

            List<string> listResp = new List<string>();
            foreach (CheckBox temp in PnlResp.Controls.OfType<CheckBox>())
            {
                if (temp.Enabled && temp.Checked)
                {
                    listResp.Add((string)temp.Tag);
                }
            }
            
            FormMain.ConfigurarContinentes(listContinentes);
            FormMain.ConfigurarPreguntas(listPreguntas);
            FormMain.ConfigurarRespuestas(listResp);

            List<List<string>> listas = new List<List<string>>();
            listas.Add(listContinentes);
            listas.Add(listPreguntas);
            listas.Add(listResp);

            EscribirConfig(listas);
           
            this.Close();
        }

        private void CrearFile(string file)
        {
            if (!File.Exists("config.dat"))
            {
               FileStream fs = File.Create("config.dat");
                fs.Close();
            }            
        }

        private void EscribirConfig(List<List<string>> listas)
        {
            CrearFile("config.dat");
            
            using (Stream stream = new FileStream("config.dat", FileMode.Open, FileAccess.Write)) 
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, listas);
                stream.Close();
            }
            
           
        }

        
    }
}
