using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Configuration;



namespace FarmaciaDonBosco
{
    public partial class frmControlUsuarios : Form
    {
        private string connectionString;
        private string usuarioActual;

        public frmControlUsuarios()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["FarmaciaDonBosco"].ConnectionString;

        }

        private void Form6_Load(object sender, EventArgs e)
        {
            // Añade los roles disponibles al ComboBox
            cmbRol.Items.Add("Administrador");
            cmbRol.Items.Add("Empleado");
            cmbRol.SelectedIndex = 0;

            // Cargar los usuarios desde la base de datos
            CargarUsuarios();
        }

        // Método para cargar los usuarios desde la base de datos y mostrarlos en el DataGridView
        private void CargarUsuarios()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT nombreUsuario, contraseña, rol FROM Usuarios";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable(); // Crea un DataTable para almacenar los resultados
                    adapter.Fill(dt); // Llena el DataTable con los resultados de la consulta
                    dgvUsuarios.DataSource = dt; // Asigna el DataTable como fuente de datos para el DataGridView

                    // Reemplazar la contraseña por asteriscos
                    foreach (DataRow row in dt.Rows)
                    {
                        row["contraseña"] = new string('*', row["contraseña"].ToString().Length);
                    }

                    dgvUsuarios.Columns["contraseña"].HeaderText = "Contraseña";
                    dgvUsuarios.Columns["nombreUsuario"].HeaderText = "Nombre Usuario";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Evento que se activa al hacer clic en el botón Agregar Usuario
        private void btnAgregarUsuario_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text) ||
                    string.IsNullOrWhiteSpace(txtContraseña.Text))
                {
                    MessageBox.Show("Por favor, complete todos los campos requeridos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Abre la conexión a la base de datos para agregar un nuevo usuario
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Usuarios (nombreUsuario, contraseña, rol) VALUES (@nombreUsuario, @contraseña, @rol)";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    // Asigna los parámetros de la consulta
                    cmd.Parameters.AddWithValue("@nombreUsuario", txtNombreUsuario.Text);
                    cmd.Parameters.AddWithValue("@contraseña", txtContraseña.Text);
                    cmd.Parameters.AddWithValue("@rol", cmbRol.SelectedItem.ToString());

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Usuario agregado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Llama al método para cargar los usuarios en el DataGridView
                    CargarUsuarios();

                    LimpiarCampos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar el usuario: " + ex.Message);
            }
        }

        // Método para limpiar los campos del formulario
        private void LimpiarCampos()
        {
            txtNombreUsuario.Clear();
            txtContraseña.Clear();
            cmbRol.SelectedIndex = 0;
            usuarioActual = null;
        }

        // Evento que se activa al hacer clic en el botón Editar Usuario
        private void btnEditarUsuario_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 1)
            {
                DataGridViewRow filaSeleccionada = dgvUsuarios.SelectedRows[0];

                // Verifica que los valores no sean null antes de acceder
                if (filaSeleccionada.Cells[0].Value != null && filaSeleccionada.Cells[1].Value != null)
                {
                    // Cargar los datos seleccionados en los TextBoxes
                    txtNombreUsuario.Text = filaSeleccionada.Cells[0].Value.ToString();
                    txtContraseña.Text = "****"; // Mostrar asteriscos
                    cmbRol.SelectedItem = filaSeleccionada.Cells[2].Value.ToString(); // Rol

                    // Guardar el usuario actual para actualizarlo después
                    usuarioActual = filaSeleccionada.Cells[0].Value.ToString();

                    btnGuardar.Enabled = true; 
                }
                else
                {
                    MessageBox.Show("No se pueden editar campos vacíos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Seleccione un usuario para editar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Evento que se activa al hacer clic en el botón Eliminar Usuario
        private void btnEliminarUsuario_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 1)
            {
                DialogResult resultado = MessageBox.Show("¿Está seguro de que desea eliminar este usuario?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resultado == DialogResult.Yes)
                {
                    DataGridViewRow filaSeleccionada = dgvUsuarios.SelectedRows[0];
                    string nombreUsuario = filaSeleccionada.Cells[0].Value.ToString();

                    // Eliminar usuario de la base de datos
                    EliminarUsuario(nombreUsuario);

                    // Actualizar DataGridView
                    CargarUsuarios();
                }
            }
            else
            {
                MessageBox.Show("Seleccione un usuario para eliminar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Método para eliminar un usuario de la base de datos
        private void EliminarUsuario(string nombreUsuario)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Usuarios WHERE nombreUsuario = @nombreUsuario"; // Consulta para eliminar el usuario
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@nombreUsuario", nombreUsuario);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el usuario: " + ex.Message);
            }
        }

        // Evento que se activa al hacer clic en el botón Cambiar Contraseña
        private void btnCambiarContraseña_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 1)
            {
                // Mostrar TextBox y Button para ingresar la nueva contraseña
                txtNuevaContraseña.Visible = true;
                btnConfirmarCambio.Visible = true;

                btnConfirmarCambio.Click += (s, ev) =>
                {
                    // Verifica que la nueva contraseña no esté vacía
                    if (!string.IsNullOrEmpty(txtNuevaContraseña.Text))
                    {
                        DataGridViewRow filaSeleccionada = dgvUsuarios.SelectedRows[0];
                        string nombreUsuario = filaSeleccionada.Cells[0].Value.ToString();
                        ActualizarContraseña(nombreUsuario, txtNuevaContraseña.Text);
                        MessageBox.Show("Contraseña actualizada correctamente.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Ocultar los controles de nuevo
                        txtNuevaContraseña.Visible = false;
                        btnConfirmarCambio.Visible = false;
                        txtNuevaContraseña.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Ingrese una contraseña válida.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
            }
            else
            {
                MessageBox.Show("Seleccione un usuario para cambiar la contraseña.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Método para cambiar la contraseña de un usuario en la base de datos
        private void ActualizarContraseña(string nombreUsuario, string nuevaContraseña)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Usuarios SET contraseña = @nuevaContraseña WHERE nombreUsuario = @nombreUsuario";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@nuevaContraseña", nuevaContraseña);
                    cmd.Parameters.AddWithValue("@nombreUsuario", nombreUsuario);
                    int filasAfectadas = cmd.ExecuteNonQuery(); // Captura el número de filas afectadas

                    if (filasAfectadas > 0)
                    {
                        MessageBox.Show("Contraseña actualizada correctamente.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("No se encontró el usuario para actualizar la contraseña.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar la contraseña: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close(); // Cierra el formulario de generación de factura
        }

        // Evento que se activa al hacer clic en el botón Guardar Cambios
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (usuarioActual == null)
            {
                MessageBox.Show("Seleccione un usuario para editar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Verifica que los campos no estén vacíos
            if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text) ||
                string.IsNullOrWhiteSpace(txtContraseña.Text) ||
                cmbRol.SelectedItem == null)
            {
                MessageBox.Show("Por favor, complete todos los campos requeridos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // Actualizar nombre de usuario, contraseña y rol
                    string query = "UPDATE Usuarios SET nombreUsuario = @nuevoNombreUsuario, contraseña = @contraseña, rol = @rol WHERE nombreUsuario = @nombreUsuario";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@nuevoNombreUsuario", txtNombreUsuario.Text);
                    cmd.Parameters.AddWithValue("@contraseña", txtContraseña.Text);
                    cmd.Parameters.AddWithValue("@rol", cmbRol.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@nombreUsuario", usuarioActual); // Usuario actual que se edita

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Usuario actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Llama al método para cargar los usuarios en el DataGridView
                    CargarUsuarios();

                    // Limpiar campos después de guardar
                    LimpiarCampos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el usuario: " + ex.Message);
            }
        }
        private bool NombreUsuarioExiste(string nuevoNombreUsuario)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Usuarios WHERE nombreUsuario = @nombreUsuario";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombreUsuario", nuevoNombreUsuario);

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0; // Retorna true si existe
            }
        }

    }

}
    
    
