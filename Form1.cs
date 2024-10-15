using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using MySql.Data.MySqlClient;


namespace FarmaciaDonBosco
{
    public partial class frmInicio : Form
    {
        public frmInicio()

        {
            InitializeComponent();
            timer1.Interval = 3000;  // Intervalo de 3 segundos
            timer1.Start();  // Inicia el timer al abrir el formulario
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Detiene el timer una vez que se ha activado
            timer1.Stop();
            // Oculta el formulario actual
            this.Hide();
            // Crea una nueva instancia del formulario de login
            frmLogin login = new frmLogin();
            login.ShowDialog();
            this.Close();
        }

        private void frmInicio_Load(object sender, EventArgs e)
        {

        }
    }
}
