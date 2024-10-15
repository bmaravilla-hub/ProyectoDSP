using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace FarmaciaDonBosco
{
    public partial class frmGestionInventario : Form
    {
        private string connectionString;
        private int productoId; // Variable para almacenar el ID del producto seleccionado

        public frmGestionInventario()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["FarmaciaDonBosco"].ConnectionString;
            CargarProductos();

        }

        private void CargarProductos()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Inventario"; // Consulta para seleccionar todos los productos del inventario
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt); // Llena el DataTable con los datos de la consulta
                    dataGridView1.DataSource = dt; // Asigna el DataTable al DataGridView para mostrar los productos
                }
            }
            
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message);
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // Consulta para insertar un nuevo producto en el inventario
                    string query = @"INSERT INTO Inventario (marca, nombre, Precio, FechaVencimiento, CantidadDisponible) 
                                     VALUES (@marca, @nombre, @precio, @fechaVencimiento, @cantidadDisponible)";
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    // Validar y asignar los valores de los campos
                    decimal precio;
                    int cantidadDisponible;

                    // Validación para el precio
                    if (!decimal.TryParse(txtPrecio.Text, out precio))
                    {
                        MessageBox.Show("El precio debe ser un valor numérico válido.");
                        return;
                    }

                    // Validación para la cantidad disponible

                    if (!int.TryParse(txtCantidadDisponible.Text, out cantidadDisponible))
                    {
                        MessageBox.Show("La cantidad disponible debe ser un número entero válido.");
                        return;
                    }

                    // Asignación de parámetros a la consulta
                    cmd.Parameters.AddWithValue("@marca", txtMarca.Text);
                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@precio", precio); 
                    cmd.Parameters.AddWithValue("@fechaVencimiento", dtpFechaVencimiento.Value);
                    cmd.Parameters.AddWithValue("@cantidadDisponible", cantidadDisponible);

                    cmd.ExecuteNonQuery(); 
                    MessageBox.Show("Producto agregado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Limpiar campos después de agregar
                    LimpiarCampos();

                    // Volver a cargar los productos para mostrar el nuevo producto en el DataGridView
                    CargarProductos();
                }
            }
            catch (MySqlException ex)
            {
                // Mostrar mensaje de error con más detalles de MySQL
                MessageBox.Show("Error de base de datos: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void LimpiarCampos()
        {
            txtMarca.Clear();
            txtNombre.Clear();
            txtPrecio.Clear();
            txtCantidadDisponible.Clear();
            dtpFechaVencimiento.Value = DateTime.Now; 
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                // Obtiene la fila seleccionada
                DataGridViewRow filaSeleccionada = dataGridView1.SelectedRows[0];


                txtMarca.Text = filaSeleccionada.Cells[1].Value.ToString(); 
                txtNombre.Text = filaSeleccionada.Cells[2].Value.ToString(); 
                txtPrecio.Text = filaSeleccionada.Cells[3].Value.ToString(); 

                // Validar que el valor en la columna de fecha sea de tipo DateTime (Fecha de vencimiento)
                if (DateTime.TryParse(filaSeleccionada.Cells[4].Value?.ToString(), out DateTime fechaVencimiento))
                {
                    dtpFechaVencimiento.Value = fechaVencimiento;
                }
                else
                {
                    MessageBox.Show("El valor de la fecha de vencimiento no es válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                txtCantidadDisponible.Text = filaSeleccionada.Cells[5].Value.ToString(); 

                // Almacena el ID del producto seleccionado
                productoId = Convert.ToInt32(filaSeleccionada.Cells[0].Value);
            }
            else
            {
                MessageBox.Show("Seleccione un medicamento para editar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                DialogResult resultado = MessageBox.Show("¿Está seguro de que desea eliminar este medicamento?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resultado == DialogResult.Yes)
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
                }
            }
            else
            {
                MessageBox.Show("Seleccione un medicamento para eliminar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close(); // Cierra el formulario de gestión de inventario
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // Consulta para actualizar un producto existente en el inventario
                    string query = @"UPDATE Inventario 
                             SET marca = @marca, nombre = @nombre, Precio = @precio, FechaVencimiento = @fechaVencimiento, CantidadDisponible = @cantidadDisponible 
                             WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    // Validar y asignar los valores de los campos
                    decimal precio;
                    int cantidadDisponible;

                    // Validación para el precio
                    if (!decimal.TryParse(txtPrecio.Text, out precio))
                    {
                        MessageBox.Show("El precio debe ser un valor numérico válido.");
                        return;
                    }

                    // Validación para la cantidad disponible
                    if (!int.TryParse(txtCantidadDisponible.Text, out cantidadDisponible))
                    {
                        MessageBox.Show("La cantidad disponible debe ser un número entero válido.");
                        return;
                    }

                    // Asignación de parámetros a la consulta
                    cmd.Parameters.AddWithValue("@marca", txtMarca.Text);
                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@precio", precio);
                    cmd.Parameters.AddWithValue("@fechaVencimiento", dtpFechaVencimiento.Value);
                    cmd.Parameters.AddWithValue("@cantidadDisponible", cantidadDisponible);
                    cmd.Parameters.AddWithValue("@id", productoId); 

                    cmd.ExecuteNonQuery(); 
                    MessageBox.Show("Producto actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LimpiarCampos();

                    CargarProductos();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error de base de datos: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
