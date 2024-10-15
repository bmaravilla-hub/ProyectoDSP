using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FarmaciaDonBosco
{
    public class Productos
    {
        public class Producto
        {
            public string Nombre { get; set; }
            public decimal Precio { get; set; }

            public override string ToString()
            {
                return Nombre; 
            }
        }

        public List<Producto> ListaProductos { get; set; } = new List<Producto>();

        // Método para agregar un producto a la lista
        public void AgregarProducto(string nombre, decimal precio)
        {
            ListaProductos.Add(new Producto { Nombre = nombre, Precio = precio });
        }

        // Método para obtener todos los productos
        public List<Producto> ObtenerProductos()
        {
            return ListaProductos;
        }
    }
}
