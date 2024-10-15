using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;


namespace FarmaciaDonBosco
{
    public partial class frmMenuPrincipal : Form
    {
        public frmMenuPrincipal()
        {
            InitializeComponent();
        }

        private void btnInventario_Click(object sender, EventArgs e)
        {
            frmGestionInventario gestionInventario = new frmGestionInventario();
            gestionInventario.ShowDialog();
        }

        private void btnFacturacion_Click(object sender, EventArgs e)
        {
            frmGenerarFactura generarFactura = new frmGenerarFactura();
            generarFactura.ShowDialog();
        }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            frmControlUsuarios controlUsuarios = new frmControlUsuarios();
            controlUsuarios.ShowDialog();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            // Muestra un mensaje de confirmación antes de cerrar la aplicación
            DialogResult resultado = MessageBox.Show("¿Estás seguro de que deseas salir?", "Confirmar Salida", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
