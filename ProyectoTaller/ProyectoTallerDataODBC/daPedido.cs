﻿using ProyectoTallerDataODBC;
using ProyectoTallerEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;

namespace ProyectoTallerData {
    public class daPedido {
        private const string SQLSearchByPrimaryKey = "SELECT * FROM Pedidos WHERE IdPedido = ?";
        private const string SQLSearch = "SELECT * FROM Pedidos WHERE IdCliente = ?";
        private const string SQLInsert = "INSERT INTO Pedidos (IdCliente, Fecha, Estado) VALUES (?, ?, ?)";
        private const string SQLUpdate = "UPDATE Pedidos SET IdCliente = ?, Fecha = ?, Estado = ? WHERE IdPedido = ?";
        private const string SQLDelete = "DELETE FROM Pedidos WHERE IdPedido = ?";
        private const string SQLItemsCarrito = "SELECT pro.idproducto, imagen, cantidad, nombre, precio, modelo, iddetalle FROM pedidos ped INNER JOIN detalles det ON det.idpedido = ped.idpedido INNER JOIN productos pro ON pro.idproducto = det.idproducto WHERE ped.idpedido = ?";
        private const string SQLPedidoAbierto = "SELECT idpedido FROM Pedidos P INNER JOIN Clientes C ON C.idcliente = P.idcliente WHERE P.estado = 5 AND idusuario = ?";

        private daConexion connectionDA = new daConexion();

        public daPedido() {}

        public PedidoEntity CrearEntidad(OdbcDataReader dr) {
            PedidoEntity entidad = new PedidoEntity();
            entidad.IdPedido = Convert.ToInt32(dr["IdPedido"]);
            entidad.IdCliente = Convert.ToInt32(dr["IdCliente"]);
            entidad.Fecha = Convert.ToDateTime(dr["Fecha"]);
            entidad.Estado = Convert.ToInt32(dr["Estado"]);
            entidad.Detalles = new daDetalle().ObtenerDetallesPorPedido(entidad.IdPedido);
            return entidad;
        }

        private void CrearParametros(OdbcCommand command, PedidoEntity entidad) {
            OdbcParameter parameter = null;

            parameter = command.Parameters.Add("?", OdbcType.Int);
            parameter.Value = entidad.IdPedido;

            parameter = command.Parameters.Add("?", OdbcType.Int);
            parameter.Value = entidad.IdCliente;

            parameter = command.Parameters.Add("?", OdbcType.DateTime);
            parameter.Value = entidad.Fecha;

            parameter = command.Parameters.Add("?", OdbcType.VarChar);
            parameter.Value = entidad.Estado;
        }

        private void EjecutarComando(daComun.TipoComandoEnum sqlCommandType, PedidoEntity entidad) {
            OdbcConnection connection = null;
            OdbcCommand command = null;

            try {
                connection = (OdbcConnection) connectionDA.GetOpenedConnection();
                IDataParameter paramId = new OdbcParameter("?", OdbcType.Int);
                paramId.Value = entidad.IdPedido;

                switch(sqlCommandType) {
                    case daComun.TipoComandoEnum.Insertar:
                        command = new OdbcCommand(SQLInsert, connection);
                        command.Parameters.Add(paramId);
                        CrearParametros(command, entidad);
                        break;

                    case daComun.TipoComandoEnum.Actualizar:
                        command = new OdbcCommand(SQLUpdate, connection);
                        command.Parameters.Add(paramId);
                        CrearParametros(command, entidad);
                        break;

                    case daComun.TipoComandoEnum.Eliminar:
                        command = new OdbcCommand(SQLDelete, connection);
                        command.Parameters.Add(paramId);
                        CrearParametros(command, entidad);
                        break;
                }

                command.ExecuteNonQuery();
                connection.Close();
            } catch(Exception ex) {
                throw new daException(ex);
            } finally {
                if(command != null) {
                    command.Dispose();
                }

                if(connection != null) {
                    connection.Dispose();
                }
            }
        }

        public int ObtenerPedidoAbierto(int idusuario) {
            OdbcConnection connection = null; // Conexión a la base de datos
            OdbcCommand command = null; // Comando a ejecutar en la base de datos.
            OdbcDataReader dr = null; // DataReader con registros de datos.
            int pedido = 0;

            try {
                connection = (OdbcConnection) connectionDA.GetOpenedConnection(); // Se obtiene una conexión abierta.
                command = new OdbcCommand(SQLPedidoAbierto, connection); // Se crea el comando con la sentencia Select.
                command.Parameters.Add("?", OdbcType.Int); // Se agrega el parámetro idcliente.
                command.Parameters[0].Value = idusuario;
                dr = command.ExecuteReader();

                while(dr.Read()) {
                    pedido = Convert.ToInt32(dr["idpedido"]);
                }

                connection.Close(); // Se cierra la conexión.
            } catch(Exception ex) {
                throw new daException(ex);
            } finally {
                if(command != null) {
                    command.Dispose();
                }
                if(connection != null) {
                    connection.Dispose();
                }
            }

            return pedido;
        }

        public PedidoEntity ObtenerPedido(int idpedido) {
            OdbcConnection connection = null; // Conexión a la base de datos
            OdbcCommand command = null; // Comando a ejecutar en la base de datos.
            OdbcDataReader dr = null; // DataReader con registros de datos.
            PedidoEntity pedido = null;

            try {
                connection = (OdbcConnection) connectionDA.GetOpenedConnection(); // Se obtiene una conexión abierta.
                command = new OdbcCommand(SQLSearchByPrimaryKey, connection); // Se crea el comando con la sentencia Select.
                command.Parameters.Add("?", OdbcType.Int); // Se agrega el parámetro idcliente.
                command.Parameters[0].Value = idpedido;
                dr = command.ExecuteReader();

                while(dr.Read()) {
                    pedido = CrearEntidad(dr);
                }

                connection.Close(); // Se cierra la conexión.
            } catch(Exception ex) {
                throw new daException(ex);
            } finally {
                if(command != null) {
                    command.Dispose();
                }
                if(connection != null) {
                    connection.Dispose();
                }
            }

            return pedido;
        }

        public DataTable ObtenerPedidoCarrito(int idpedido) {
            OdbcConnection connection = null; // Conexión a la base de datos
            OdbcCommand command = null; // Comando a ejecutar en la base de datos.
            OdbcDataAdapter da = null;
            DataTable dt = new DataTable();

            try {
                connection = (OdbcConnection) connectionDA.GetOpenedConnection(); // Se obtiene una conexión abierta.
                command = new OdbcCommand(SQLItemsCarrito, connection); // Se crea el comando con la sentencia Select.
                command.Parameters.Add("?", OdbcType.Int); // Se agrega el parámetro idcliente.
                command.Parameters[0].Value = idpedido;
                da = new OdbcDataAdapter(command);
                da.Fill(dt);

                connection.Close(); // Se cierra la conexión.
            } catch (Exception ex) {
                throw new daException(ex);
            } finally {
                if(command != null) {command.Dispose();}
                if(connection != null) {connection.Dispose();}
            }

            return dt;
        }

        public List<PedidoEntity> ObtenerPedidosPorCliente(int idcliente) {
            OdbcConnection connection = null; // Conexión a la base de datos
            OdbcCommand command = null; // Comando a ejecutar en la base de datos.
            OdbcDataReader dr = null; // DataReader con registros de datos.
            List<PedidoEntity> pedidos = null; // Lista de objetos con datos de empleados.

            try {
                connection = (OdbcConnection) connectionDA.GetOpenedConnection(); // Se obtiene una conexión abierta.
                command = new OdbcCommand(SQLSearch, connection); // Se crea el comando con la sentencia Select.
                command.Parameters.Add("?", OdbcType.Int); // Se agrega el parámetro idcliente.
                command.Parameters[0].Value = idcliente;
                // Se ejecuta el comando SQL en la base de datos y se devuelve 
                // una instancia de DataReader con los registros encontrados.
                dr = command.ExecuteReader();

                pedidos = new List<PedidoEntity>(); // Se crea una instancia de la lista de empleados.

                while(dr.Read()) // Mientras que se pueda leer el DataReader.
                {
                    pedidos.Add(CrearEntidad(dr)); // Se agrega un objeto con los datos del empleado a la lista.
                }

                dr.Close(); // Se cierra el DataReader.
                connection.Close(); // Se cierra la conexión.
            } catch(Exception ex) {
                throw new daException(ex);
            } finally {
                dr = null;

                if(command != null) {
                    command.Dispose(); // Se libera el recurso.
                }

                if(connection != null) {
                    connection.Dispose(); // Se libera el recursos.
                }
            }

            return pedidos;
        }

        public void Insertar(PedidoEntity entidad) {
            daContadores da = new daContadores();
            entidad.IdPedido = da.TraerContador(daComun.Contador.Pedido) + 1;
            EjecutarComando(daComun.TipoComandoEnum.Insertar, entidad);
            da.Sumar(daComun.Contador.Pedido);

            daDetalle detalles = new daDetalle();
            foreach(DetalleEntity detalle in entidad.Detalles) {
                detalles.Insertar(detalle);
            }
        }

        public void Actualizar(PedidoEntity entidad) {
            EjecutarComando(daComun.TipoComandoEnum.Actualizar, entidad);

            daDetalle detalles = new daDetalle();
            foreach(DetalleEntity detalle in entidad.Detalles) {
                detalles.Actualizar(detalle);
            }
        }

        public void Eliminar(int id) {
            PedidoEntity entidad = new PedidoEntity();
            entidad.IdPedido = id;
            EjecutarComando(daComun.TipoComandoEnum.Eliminar, entidad);
            new daDetalle().EliminarPorPedido(id);
        }

    }
}
