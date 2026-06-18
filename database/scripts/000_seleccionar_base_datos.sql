-- Script auxiliar para trabajar sobre la base correcta.
-- Ejecutalo al inicio de una ventana de consulta antes de correr scripts
-- que no incluyen USE Plataforma_Empleabilidad_BD.

USE Plataforma_Empleabilidad_BD;
GO

SELECT DB_NAME() AS BaseDeDatosActual;
GO
