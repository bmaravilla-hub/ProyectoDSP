using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Configuration;
using static FarmaciaDonBosco.Productos;

namespace FarmaciaDonBosco
{
    public partial class frmGenerarFactura : Form

    {
        private string connectionString;
        private MySqlConnection connection; 
        private Dictionary<string, (int cantidad, decimal subtotal)> productosComprados;


        public frmGenerarFactura()
        {

            InitializeComponent();

            //Tipos de pago
            cmbTipoPago.Items.Add("Contado");
            cmbTipoPago.Items.Add("Tarjeta");
            cmbTipoPago.SelectedIndex = 0;

            productosComprados = new Dictionary<string, (int cantidad, decimal subtotal)>();
            connectionString = ConfigurationManager.ConnectionStrings["FarmaciaDonBosco"].ConnectionString;
            connection = new MySqlConnection(connectionString);
            CargarProductos();

            // Agregar evento al seleccionar un producto
            comboBoxProductos.SelectedIndexChanged += cmbProducto_SelectedIndexChanged;
        }

        // Método que se ejecuta al hacer clic en el botón para agregar un producto
        private void btnAgregarProducto_Click(object sender, EventArgs e)
        {
            try
            {
                // Verifica el contenido del ComboBox
                Console.WriteLine("Contenido del ComboBox:");
                foreach (var item in comboBoxProductos.Items)
                {
                    Console.WriteLine(item.ToString()); 
                }

                // Validar que se ha seleccionado un producto
                if (comboBoxProductos.SelectedItem == null)
                {
                    MessageBox.Show("Por favor, selecciona un producto antes de agregarlo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Valida que la cantidad ingresada sea un número válido
                if (string.IsNullOrWhiteSpace(txtCantidad.Text) || !int.TryParse(txtCantidad.Text, out int cantidad))
                {
                    MessageBox.Show("Por favor, ingresa una cantidad válida antes de agregar el producto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Obtener el producto seleccionado desde el ComboBox como un objeto Producto
                Productos.Producto productoSeleccionado = (Productos.Producto)comboBoxProductos.SelectedItem;
                Console.WriteLine("Producto seleccionado: " + productoSeleccionado.Nombre);

                // Obtener los detalles del producto
                string nombreProducto = productoSeleccionado.Nombre;
                decimal precioProducto = productoSeleccionado.Precio;

                // Calcular el subtotal
                decimal subtotalProducto = cantidad * precioProducto;

                // Acumular la cantidad y el subtotal en el diccionario
                if (productosComprados.ContainsKey(nombreProducto))
                {
                    var info = productosComprados[nombreProducto];
                    productosComprados[nombreProducto] = (info.cantidad + cantidad, info.subtotal + subtotalProducto);
                }
                else
                {
                    productosComprados[nombreProducto] = (cantidad, subtotalProducto);
                }

                // Calcular el total después de aplicar el descuento
                decimal descuento = ObtenerDescuento();
                decimal total = subtotalProducto - descuento;

                // Actualizar el DataGridView
                string nombreCliente = txtNombreCliente.Text;
                dgvFactura.Rows.Add(nombreCliente, nombreProducto, cantidad, precioProducto.ToString("C"), subtotalProducto.ToString("C"), total.ToString("C"));

                // Actualizar los campos de subtotal y total en el formulario
                ActualizarSubtotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar producto: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Método para obtener el precio unitario de un producto
        private decimal ObtenerPrecioUnitario(string producto)
        {
            decimal precio = 0;

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand("SELECT precio FROM Inventario WHERE nombre = @producto", connection);
                    command.Parameters.AddWithValue("@producto", producto);

                    var resultado = command.ExecuteScalar();
                    if (resultado != null)
                    {
                        precio = Convert.ToDecimal(resultado);
                    }
                    else
                    {
                        MessageBox.Show("No se encontró el precio del producto.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener el precio unitario: " + ex.Message);
            }

            return precio;
        }

        // Método para actualizar el subtotal de la factura
        private void ActualizarSubtotal()
        {
            // Calcular el subtotal sumando los subtotales de todos los productos
            decimal subtotal = 0;
            foreach (var producto in productosComprados)
            {
                subtotal += producto.Value.subtotal; // Suma los subtotales de cada producto
            }

            // Actualizar el campo Subtotal en el formulario
            txtSubtotal.Text = subtotal.ToString("C");

            // Solo actualizar el total si se ha seleccionado un descuento
            if (cmbDescuentos.SelectedItem != null)
            {
                ActualizarTotalConDescuento();
            }
            else
            {
                txtTotal.Text = subtotal.ToString("C"); // Si no hay descuento, el total es igual al subtotal
            }
        }

        // Evento que se activa al hacer clic en el botón para generar la factura
        private void btnGenerarFactura_Click(object sender, EventArgs e)
        {
            GenerarFactura();
        }

        // Método para cargar los productos en el ComboBox desde la base de datos
        private void CargarProductos()
        {
            comboBoxProductos.Items.Clear(); 
            string query = "SELECT nombre, precio FROM Inventario"; // Consulta SQL para obtener los productos

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) // Lee los resultados
                {
                    Producto producto = new Producto // Crea un nuevo objeto Producto
                    {
                        Nombre = reader["nombre"].ToString(),
                        Precio = Convert.ToDecimal(reader["precio"])
                    };
                    comboBoxProductos.Items.Add(producto); // Agrega el producto al ComboBox
                }
                reader.Close();
            }
        }

        // Método para ejecutar un comando SQL con parámetros
        private void EjecutarComando(string query, Dictionary<string, object> parameters)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    // Agregar los parámetros a la consulta
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        MessageBox.Show("Factura guardada exitosamente.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al guardar la factura: " + ex.Message);
                    }
                }
            }
        }

        // Método para guardar la factura en la base de datos
        private void GuardarFactura()
        {
            // Validar el nombre del cliente
            string cliente = txtNombreCliente.Text.Trim();
            if (string.IsNullOrWhiteSpace(cliente))
            {
                MessageBox.Show("El campo Cliente no puede estar vacío.");
                return;
            }

            // Validar la selección del producto
            string producto = comboBoxProductos.SelectedItem?.ToString();
            if (producto == null)
            {
                MessageBox.Show("Por favor, seleccione un producto.");
                return;
            }

            // Validar la cantidad y convertirla a entero
            if (!int.TryParse(txtCantidad.Text.Trim(), out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Por favor, ingrese una cantidad válida.");
                return;
            }

            // Validar el precio unitario y convertirlo a decimal
            if (!decimal.TryParse(txtprecioUnitario.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precioUnitario))
            {
                MessageBox.Show("Por favor, ingrese un precio unitario válido.");
                return;
            }

            // Calcular el subtotal
            decimal subtotal = precioUnitario * cantidad;

            // Validar el descuento seleccionado en el ComboBox
            decimal descuento = 0;
            switch (cmbDescuentos.SelectedItem?.ToString())
            {
                case "Sin descuento":
                    descuento = 0;
                    break;
                case "10%":
                    descuento = 0.10m;
                    break;
                case "15%":
                    descuento = 0.15m;
                    break;
                case "20%":
                    descuento = 0.20m;
                    break;
                default:
                    MessageBox.Show("Por favor, seleccione un descuento válido.");
                    return;
            }

            // Calcular el total con descuento
            decimal totalConDescuento = subtotal * (1 - descuento);

            // Asignar el total al campo txtTotal en formato adecuado
            txtTotal.Text = totalConDescuento.ToString("F2", CultureInfo.InvariantCulture);

            // Guarda los detalles de la factura en la base de datos
            string query = "INSERT INTO Factura (cliente, producto, precioUnitario, cantidad, total) VALUES (@cliente, @producto, @precioUnitario, @cantidad, @total)";

            // Crear los parámetros para la consulta SQL
            var parameters = new Dictionary<string, object>
            {
                { "@cliente", cliente },
                { "@producto", producto },
                { "@precioUnitario", precioUnitario },
                { "@cantidad", cantidad },
                { "@total", totalConDescuento }
            };

            // Ejecutar el comando SQL con los parámetros
            EjecutarComando(query, parameters);

            MessageBox.Show("Factura guardada exitosamente.");
        }

        // Método que genera la factura y la guarda en un archivo de tex
        private void GenerarFactura()
        {
            try
            {
                // Validar si los controles contienen datos válidos
                if (!EsFacturaValida())
                {
                    MessageBox.Show("Por favor, completa todos los campos requeridos antes de generar la factura.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Guardar la factura en la base de datos
                GuardarFactura();

                // Definir la ruta para guardar la factura
                string rutaFactura = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Factura.txt");

                // Generar archivo de texto con la factura
                using (StreamWriter writer = new StreamWriter(rutaFactura, true))
                {
                    writer.WriteLine("Factura");
                    writer.WriteLine($"Cliente: {txtNombreCliente.Text}");
                    foreach (var producto in productosComprados)
                    {
                        writer.WriteLine($"Producto: {producto.Key}, Cantidad: {producto.Value.cantidad}, Subtotal: {producto.Value.subtotal.ToString("C")}");
                    }
                    writer.WriteLine($"Subtotal: {txtSubtotal.Text}");
                    writer.WriteLine($"Total: {txtTotal.Text}");
                    writer.WriteLine("------------------------------------------");
                }

                // Mostrar la ruta donde se guardó la factura
                MessageBox.Show($"Factura generada y guardada en: {rutaFactura}", "Factura Generada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar la factura: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool EsFacturaValida()
        {
            return productosComprados.Count > 0 &&
                   !string.IsNullOrWhiteSpace(txtNombreCliente.Text) &&
                   !string.IsNullOrWhiteSpace(txtSubtotal.Text) &&
                   !string.IsNullOrWhiteSpace(txtTotal.Text);
        }


        private void ActualizarTotalConDescuento()
        {
            if (decimal.TryParse(txtSubtotal.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal subtotal))
            {
                decimal descuento = ObtenerDescuento();
                decimal total = subtotal - descuento;

                txtTotal.Text = total.ToString("C");
            }
        }

        private decimal ObtenerDescuento()
        {
            decimal descuento = 0;
            decimal subtotal;

            if (decimal.TryParse(txtSubtotal.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out subtotal))
            {
                switch (cmbDescuentos.SelectedItem?.ToString())
                {
                    case "10%":
                        descuento = subtotal * 0.10m;
                        break;
                    case "15%":
                        descuento = subtotal * 0.15m;
                        break;
                    case "20%":
                        descuento = subtotal * 0.20m;
                        break;
                    default:
                        break; // Sin descuento
                }
            }

            return descuento;
        }


        private void frmGenerarFactura_Load(object sender, EventArgs e)
        {
            if (dgvFactura.Columns.Count == 0)
            {
                dgvFactura.Columns.Add("Cliente", "Cliente");
                dgvFactura.Columns.Add("Producto", "Producto");
                dgvFactura.Columns.Add("Cantidad", "Cantidad");
                dgvFactura.Columns.Add("Precio", "Precio");
                dgvFactura.Columns.Add("Subtotal", "Subtotal");
                dgvFactura.Columns.Add("Total a Pagar", "Total a Pagar");
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close(); // Cierra el formulario de generación de factura
        }

        private void cmbProducto_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxProductos.SelectedItem is Producto productoSeleccionado)
            {
                txtprecioUnitario.Text = productoSeleccionado.Precio.ToString("0.00");
            }
            else
            {
                MessageBox.Show("Por favor selecciona un producto válido.");
            }

        }

        // Método que actualiza el total con el descuento aplicado

        private void CalcularTotal()
        {
            if (decimal.TryParse(txtSubtotal.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal subtotal))
            {
                decimal descuento = ObtenerDescuento();
                decimal total = subtotal - descuento;

                // Asegúrate de que el total no sea negativo
                txtTotal.Text = total < 0 ? "0.00" : total.ToString("0.00");
            }
            else
            {
                txtTotal.Text = "0.00"; // Manejo de errores si el subtotal no es válido
            }
        }

        private void CalcularSubtotal()
        {
            if (decimal.TryParse(txtprecioUnitario.Text, out decimal precioUnitario) &&
       int.TryParse(txtCantidad.Text, out int cantidad) && cantidad > 0)
            {
                decimal subtotal = precioUnitario * cantidad;
                txtSubtotal.Text = subtotal.ToString("0.00");
                CalcularTotal(); // Asegúrate de calcular el total después de establecer el subtotal
            }
            else
            {
                txtSubtotal.Text = "0.00";
            }
        }

        private void txtCantidad_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtCantidad.Text))
            {
                CalcularSubtotal();
            }
        }


        private void txtTotal_TextChanged(object sender, EventArgs e)
        {
            ActualizarTotalConDescuento();
        }

        private void txtSubtotal_TextChanged(object sender, EventArgs e)
        {
            // Solo se actualiza el total si el subtotal tiene texto
            if (!string.IsNullOrWhiteSpace(txtSubtotal.Text))
            {
                ActualizarTotalConDescuento();
            }
        }

        private void cmbDescuentos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtSubtotal.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal subtotal))
            {
                decimal descuento = 0;

                // Determina el descuento según la selección del ComboBox
                switch (cmbDescuentos.SelectedItem?.ToString())
                {
                    case "Sin descuento":
                        descuento = 0;
                        break;
                    case "10%":
                        descuento = subtotal * 0.10m;
                        break;
                    case "15%":
                        descuento = subtotal * 0.15m;
                        break;
                    case "20%":
                        descuento = subtotal * 0.20m;
                        break;
                    default:
                        descuento = 0;
                        break;
                }

                // Calcula el total restando el descuento
                decimal total = subtotal - descuento;
                txtTotal.Text = total < 0 ? "0.00" : total.ToString("0.00"); 
            }
            else
            {
                MessageBox.Show("El subtotal no es válido.");
            }
        }

        private void txtprecioUnitario_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
