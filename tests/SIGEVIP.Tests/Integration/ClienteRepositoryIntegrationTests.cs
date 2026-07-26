using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Clientes;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Clientes;
using SIGEVIP.Infrastructure.Data;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class ClienteRepositoryIntegrationTests
    {
        [TestMethod]
        public void InsertarYObtener_ClienteValido_PersisteDatos()
        {
            DatosClientePrueba datos =
                CrearDatosPrueba();

            try
            {
                ClienteRepository repository =
                    CrearRepository();

                int idCliente =
                    repository.Insertar(
                        datos.CrearCliente());

                datos.IdCliente = idCliente;

                Cliente recuperado =
                    repository.ObtenerPorId(
                        idCliente);

                Assert.IsNotNull(recuperado);
                Assert.AreEqual(
                    datos.RazonSocial,
                    recuperado.RazonSocial);
                Assert.AreEqual(
                    datos.Cuit,
                    recuperado.Cuit);
                Assert.IsTrue(
                    recuperado.Activo);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void Actualizar_ClientePersistido_ModificaDatos()
        {
            DatosClientePrueba datos =
                CrearEInsertarCliente();

            try
            {
                ClienteRepository repository =
                    CrearRepository();

                Cliente cliente =
                    repository.ObtenerPorId(
                        datos.IdCliente);

                cliente.ActualizarDatos(
                    datos.RazonSocial + " Actualizada",
                    datos.Cuit,
                    "actualizado@sigevip.test",
                    "3415559999",
                    "Funes",
                    "Santa Fe");

                repository.Actualizar(
                    cliente);

                Cliente recuperado =
                    repository.ObtenerPorId(
                        datos.IdCliente);

                Assert.AreEqual(
                    datos.RazonSocial + " Actualizada",
                    recuperado.RazonSocial);

                Assert.AreEqual(
                    "Funes",
                    recuperado.Localidad);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void Desactivar_ClientePersistido_ConservaEstadoInactivo()
        {
            DatosClientePrueba datos =
                CrearEInsertarCliente();

            try
            {
                ClienteRepository repository =
                    CrearRepository();

                repository.Desactivar(
                    datos.IdCliente);

                Cliente recuperado =
                    repository.ObtenerPorId(
                        datos.IdCliente);

                Assert.IsFalse(
                    recuperado.Activo);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void Activar_ClienteInactivo_ConservaEstadoActivo()
        {
            DatosClientePrueba datos =
                CrearEInsertarCliente();

            try
            {
                ClienteRepository repository =
                    CrearRepository();

                repository.Desactivar(
                    datos.IdCliente);

                repository.Activar(
                    datos.IdCliente);

                Cliente recuperado =
                    repository.ObtenerPorId(
                        datos.IdCliente);

                Assert.IsTrue(
                    recuperado.Activo);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void Listar_PorRazonSocial_DevuelveCliente()
        {
            DatosClientePrueba datos =
                CrearEInsertarCliente();

            try
            {
                ClienteListadoDto resultado =
                    CrearRepository()
                        .Listar(
                            new ClienteFiltro(
                                datos.RazonSocial,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                null))
                        .SingleOrDefault(
                            actual =>
                                actual.IdCliente ==
                                datos.IdCliente);

                Assert.IsNotNull(
                    resultado);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void Listar_PorCuitNormalizado_DevuelveCliente()
        {
            DatosClientePrueba datos =
                CrearEInsertarCliente();

            try
            {
                ClienteListadoDto resultado =
                    CrearRepository()
                        .Listar(
                            new ClienteFiltro(
                                string.Empty,
                                datos.Cuit,
                                string.Empty,
                                string.Empty,
                                null))
                        .SingleOrDefault(
                            actual =>
                                actual.IdCliente ==
                                datos.IdCliente);

                Assert.IsNotNull(
                    resultado);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void Listar_PorLocalidad_DevuelveCliente()
        {
            DatosClientePrueba datos =
                CrearEInsertarCliente();

            try
            {
                ClienteListadoDto resultado =
                    CrearRepository()
                        .Listar(
                            new ClienteFiltro(
                                string.Empty,
                                string.Empty,
                                datos.Localidad,
                                string.Empty,
                                null))
                        .SingleOrDefault(
                            actual =>
                                actual.IdCliente ==
                                datos.IdCliente);

                Assert.IsNotNull(
                    resultado);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void Listar_Activos_IncluyeClienteActivo()
        {
            DatosClientePrueba datos =
                CrearEInsertarCliente();

            try
            {
                bool encontrado =
                    CrearRepository()
                        .Listar(
                            new ClienteFiltro(
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                true))
                        .Any(
                            actual =>
                                actual.IdCliente ==
                                datos.IdCliente);

                Assert.IsTrue(
                    encontrado);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void Listar_Inactivos_IncluyeClienteInactivo()
        {
            DatosClientePrueba datos =
                CrearEInsertarCliente();

            try
            {
                ClienteRepository repository =
                    CrearRepository();

                repository.Desactivar(
                    datos.IdCliente);

                bool encontrado =
                    repository
                        .Listar(
                            new ClienteFiltro(
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                false))
                        .Any(
                            actual =>
                                actual.IdCliente ==
                                datos.IdCliente);

                Assert.IsTrue(
                    encontrado);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void Insertar_CuitDuplicado_RechazaOperacion()
        {
            DatosClientePrueba datos =
                CrearEInsertarCliente();

            try
            {
                Cliente duplicado =
                    new Cliente(
                        0,
                        "Cliente duplicado",
                        datos.Cuit,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty);

                Assert.ThrowsException<ReglaNegocioException>(
                    () => CrearRepository()
                        .Insertar(
                            duplicado));
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        [TestMethod]
        public void ExisteCuit_ExcluyendoIdActual_DevuelveFalse()
        {
            DatosClientePrueba datos =
                CrearEInsertarCliente();

            try
            {
                bool existe =
                    CrearRepository()
                        .ExisteCuit(
                            datos.Cuit,
                            datos.IdCliente);

                Assert.IsFalse(
                    existe);
            }
            finally
            {
                EliminarDatosPrueba(datos);
            }
        }

        private static ClienteRepository CrearRepository()
        {
            return new ClienteRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosClientePrueba CrearEInsertarCliente()
        {
            DatosClientePrueba datos =
                CrearDatosPrueba();

            datos.IdCliente =
                CrearRepository()
                    .Insertar(
                        datos.CrearCliente());

            return datos;
        }

        private static DatosClientePrueba CrearDatosPrueba()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            string cuit =
                "30" +
                Math.Abs(
                    sufijo.GetHashCode())
                    .ToString()
                    .PadLeft(9, '0')
                    .Substring(0, 9);

            return new DatosClientePrueba(
                "Cliente Integración " +
                    sufijo,
                cuit,
                "cliente_" +
                    sufijo +
                    "@sigevip.test",
                "3415550000",
                "Rosario",
                "Santa Fe");
        }

        private static void EliminarDatosPrueba(
            DatosClientePrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            const string sql = @"
DELETE FROM dbo.Cliente
WHERE
    IdCliente = @IdCliente
    OR Cuit = @Cuit;";

            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection))
            {
                command.Parameters.Add(
                    "@IdCliente",
                    SqlDbType.Int).Value =
                        datos.IdCliente;

                command.Parameters.Add(
                    "@Cuit",
                    SqlDbType.NVarChar,
                    20).Value =
                        datos.Cuit;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static string ObtenerConnectionString()
        {
            ConnectionStringSettings settings =
                ConfigurationManager
                    .ConnectionStrings[
                        "SIGEVIP"];

            if (settings == null ||
                string.IsNullOrWhiteSpace(
                    settings.ConnectionString))
            {
                Assert.Fail(
                    "No se encontró la cadena SIGEVIP en App.config.");
            }

            return settings.ConnectionString;
        }

        private sealed class DatosClientePrueba
        {
            public DatosClientePrueba(
                string razonSocial,
                string cuit,
                string email,
                string telefono,
                string localidad,
                string provincia)
            {
                RazonSocial = razonSocial;
                Cuit = cuit;
                Email = email;
                Telefono = telefono;
                Localidad = localidad;
                Provincia = provincia;
            }

            public int IdCliente { get; set; }

            public string RazonSocial { get; private set; }

            public string Cuit { get; private set; }

            public string Email { get; private set; }

            public string Telefono { get; private set; }

            public string Localidad { get; private set; }

            public string Provincia { get; private set; }

            public Cliente CrearCliente()
            {
                return new Cliente(
                    0,
                    RazonSocial,
                    Cuit,
                    Email,
                    Telefono,
                    Localidad,
                    Provincia);
            }
        }
    }
}
