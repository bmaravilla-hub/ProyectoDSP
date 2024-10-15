CREATE DATABASE FarmaciaDonBosco;
USE FarmaciaDonBosco;

CREATE TABLE Usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombreUsuario VARCHAR(50) NOT NULL,
    contraseña VARCHAR(255) NOT NULL,
    rol ENUM('Administrador', 'Empleado') NOT NULL
);
CREATE TABLE Inventario (
    id INT AUTO_INCREMENT PRIMARY KEY,
    marca VARCHAR(50) NOT NULL,
    nombre VARCHAR(50) NOT NULL,
    precio DECIMAL(10, 2) NOT NULL,
    fechaVencimiento DATE NOT NULL,
    cantidadDisponible INT NOT NULL
);
CREATE TABLE Factura (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cliente VARCHAR(50) NOT NULL,
    producto VARCHAR(50) NOT NULL,
    precioUnitario DECIMAL(10, 2) NOT NULL,
    cantidad INT NOT NULL,
    subtotal DECIMAL(10, 2) AS (precioUnitario * cantidad) STORED,
    total DECIMAL(10, 2) NOT NULL
);

INSERT INTO Usuarios (nombreUsuario, contraseña, rol) VALUES ('admin', '1234', 'Administrador');