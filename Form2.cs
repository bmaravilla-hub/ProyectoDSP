using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;


namespace FarmaciaDonBosco
{
    public partial class frmLogin : Form
    {
        private string connectionString;

        public frmLogin()
        {
            InitializeComponent();
            // Inicializa la cadena de conexión desde el archivo de configuración
            connectionString = ConfigurationManager.ConnectionStrings["FarmaciaDonBosco"].ConnectionString;

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // Consulta SQL para verificar si el usuario y la contraseña son correctos
                    string query = "SELECT * FROM Usuarios WHERE nombreUsuario = @nombreUsuario AND contraseña = @contraseña";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@nombreUsuario", txtUsuario.Text);
                    cmd.Parameters.AddWithValue("@contraseña", txtContraseña.Text);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        // Iniciar sesión con éxito
                        MessageBox.Show("Inicio de sesión exitoso.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Hide();
                        // Crea y muestra el formulario principal del menú
                        frmMenuPrincipal menu = new frmMenuPrincipal();
                        menu.ShowDialog();
                        // Cierra el formulario de login después de que se cierre el menú principal
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Usuario o contraseña incorrectos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        } 

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            {
                Application.Exit(); // Cierra la aplicación
            }

        }
    }
 
}
