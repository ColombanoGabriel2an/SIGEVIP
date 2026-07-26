using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Visitas;
using SIGEVIP.Infrastructure.Viajes;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class ViajeVisitaRepositoryIntegrationTests
    {
        [TestMethod]
        public void ObtenerPorId_ViajeConVisita_ReconstruyeVisitaYClientes()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    2);

            try
            {
                InsertarVisita(
                    datos,
                    new[]
                    {
                        0,
                        1
                    });

                Viaje recuperado =
                    CrearViajeRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Assert.IsNotNull(
                    recuperado);

                Assert.AreEqual(
                    1,
                    recuperado.Visitas.Count);

                Visita visita =
                    recuperado.Visitas.Single();

                Assert.AreEqual(
                    datos.IdVisita,
                    visita.IdVisita);

                Assert.AreEqual(
                    datos.IdViaje,
                    visita.IdViaje);

                Assert.AreEqual(
                    datos.FechaVisita,
                    visita.Fecha);

                Assert.AreEqual(
                    datos.Observacion,
                    visita.Observacion);

                Assert.AreEqual(
                    datos.LocalidadEncuentro,
                    visita.LocalidadEncuentro);

                Assert.AreEqual(
                    2,
                    visita.Clientes.Count);

                CollectionAssert.AreEquivalent(
                    datos.IdClientes,
                    visita.Clientes
                        .Select(
                            cliente =>
                                cliente.IdCliente)
                        .ToList());
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerPorId_ClienteHistoricoInactivo_ConservaEstado()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                InsertarVisita(
                    datos,
                    new[]
                    {
                        0
                    });

                DesactivarCliente(
                    datos.IdClientes[0]);

                Viaje recuperado =
                    CrearViajeRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Cliente cliente =
                    recuperado
                        .Visitas
                        .Single()
                        .Clientes
                        .Single();

                Assert.AreEqual(
                    datos.IdClientes[0],
                    cliente.IdCliente);

                Assert.IsFalse(
                    cliente.Activo);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Cancelar_AgregadoDesactualizadoConVisitaPersistida_RechazaEnSql()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                Viaje agregadoDesactualizado =
                    CrearViajeRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Assert.AreEqual(
                    0,
                    agregadoDesactualizado
                        .Visitas
                        .Count);

                InsertarVisita(
                    datos,
                    new[]
                    {
                        0
                    });

                agregadoDesactualizado.Cancelar();

                Assert.ThrowsException<PersistenciaException>(
                    () => CrearViajeRepository()
                        .Cancelar(
                            agregadoDesactualizado));

                Viaje recuperado =
                    CrearViajeRepository()
                        .ObtenerPorId(
                            datos.IdViaje);

                Assert.AreEqual(
                    EstadoViaje.Abierto,
                    recuperado.EstadoActual);

                Assert.AreEqual(
                    1,
                    recuperado.Visitas.Count);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static DatosPrueba CrearDatosPrueba(
            int cantidadClientes)
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            var datos =
                new DatosPrueba(
                    new DateTime(
                        2032,
                        8,
                        10),
                    new DateTime(
                        2032,
                        8,
                        15),
                    new DateTime(
                        2032,
                        8,
                        12),
                    "Visita histórica " +
                        sufijo,
                    "Rosario");

            datos.IdPersona =
                InsertarPersona(
                    "Persona",
                    "ViajeVisita" +
                        sufijo.Substring(
                            0,
                            8),
                    "viaje_visita_" +
                        sufijo +
                        "@sigevip.test");

            datos.IdViaje =
                InsertarViaje(
                    datos);

            for (
                int indice = 0;
                indice < cantidadClientes;
                indice++)
            {
                datos.IdClientes.Add(
                    InsertarCliente(
                        "Cliente Viaje Visita " +
                            sufijo +
                            " " +
                            indice,
                        CrearCuit(
                            sufijo,
                            indice)));
            }

            return datos;
        }

        private static int InsertarViaje(
            DatosPrueba datos)
        {
            Persona persona =
                new Persona(
                    datos.IdPersona,
                    "Persona",
                    "ViajeVisita",
                    "persona.viajevisita@sigevip.test");

            var viaje =
                new Viaje(
                    0,
                    datos.FechaInicio,
                    datos.FechaFin,
                    "Viaje integración visitas " +
                        Guid.NewGuid()
                            .ToString("N"),
                    TipoViaje.Desplazamiento,
                    0m);

            viaje.ReemplazarParticipantes(
                new[]
                {
                    persona
                });

            return CrearViajeRepository()
                .Insertar(
                    viaje);
        }

        private static void InsertarVisita(
            DatosPrueba datos,
            IEnumerable<int> indicesClientes)
        {
            IReadOnlyCollection<Cliente> clientes =
                CrearClienteRepository()
                    .ObtenerPorIds(
                        indicesClientes
                            .Select(
                                indice =>
                                    datos.IdClientes[
                                        indice])
                            .ToList()
                            .AsReadOnly());

            var visita =
                new Visita(
                    0,
                    datos.FechaVisita,
                    datos.Observacion,
                    datos.LocalidadEncuentro);

            foreach (
                Cliente cliente
                in clientes)
            {
                visita.AgregarCliente(
                    cliente);
            }

            Viaje viaje =
                CrearViajeRepository()
                    .ObtenerPorId(
                        datos.IdViaje);

            viaje.AgregarVisita(
                visita);

            datos.IdVisita =
                CrearVisitaRepository()
                    .Insertar(
                        visita);
        }

        private static ViajeRepository
            CrearViajeRepository()
        {
            return new ViajeRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static VisitaRepository
            CrearVisitaRepository()
        {
            return new VisitaRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static ClienteConsultaVisitaRepository
            CrearClienteRepository()
        {
            return new ClienteConsultaVisitaRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static int InsertarPersona(
            string nombre,
            string apellido,
            string email)
        {
            const string sql = @"
INSERT INTO dbo.Persona
(
    Nombre,
    Apellido,
    Email,
    Activo
)
VALUES
(
    @Nombre,
    @Apellido,
    @Email,
    1
);

SELECT CAST(SCOPE_IDENTITY() AS INT);";

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
                    "@Nombre",
                    SqlDbType.NVarChar,
                    100).Value =
                        nombre;

                command.Parameters.Add(
                    "@Apellido",
                    SqlDbType.NVarChar,
                    100).Value =
                        apellido;

                command.Parameters.Add(
                    "@Email",
                    SqlDbType.NVarChar,
                    254).Value =
                        email;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int InsertarCliente(
            string razonSocial,
            string cuit)
        {
            const string sql = @"
INSERT INTO dbo.Cliente
(
    RazonSocial,
    Cuit,
    Email,
    Telefono,
    Localidad,
    Provincia,
    Activo
)
VALUES
(
    @RazonSocial,
    @Cuit,
    NULL,
    NULL,
    N'Rosario',
    N'Santa Fe',
    1
);

SELECT CAST(SCOPE_IDENTITY() AS INT);";

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
                    "@RazonSocial",
                    SqlDbType.NVarChar,
                    150).Value =
                        razonSocial;

                command.Parameters.Add(
                    "@Cuit",
                    SqlDbType.NVarChar,
                    20).Value =
                        cuit;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static string CrearCuit(
            string sufijo,
            int indice)
        {
            if (indice < 0 ||
                indice > 99)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(indice));
            }

            int baseNumerica =
                Convert.ToInt32(
                    sufijo.Substring(
                        0,
                        7),
                    16)
                % 10000000;

            return
                "31" +
                baseNumerica.ToString(
                    "D7") +
                indice.ToString(
                    "D2");
        }

        private static void DesactivarCliente(
            int idCliente)
        {
            const string sql = @"
UPDATE dbo.Cliente
SET Activo = 0
WHERE IdCliente = @IdCliente;";

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
                        idCliente;

                connection.Open();

                command.ExecuteNonQuery();
            }
        }

        private static void EliminarDatosPrueba(
            DatosPrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            {
                connection.Open();

                using (
                    SqlTransaction transaction =
                        connection.BeginTransaction())
                {
                    try
                    {
                        if (datos.IdViaje > 0)
                        {
                            EjecutarEliminacion(
                                connection,
                                transaction,
                                @"
DELETE FROM dbo.VisitaCliente
WHERE IdVisita IN
(
    SELECT IdVisita
    FROM dbo.Visita
    WHERE IdViaje = @Valor
);

DELETE FROM dbo.Visita
WHERE IdViaje = @Valor;

DELETE FROM dbo.ViajeParticipante
WHERE IdViaje = @Valor;

DELETE FROM dbo.Viaje
WHERE IdViaje = @Valor;",
                                datos.IdViaje);
                        }

                        foreach (
                            int idCliente
                            in datos.IdClientes)
                        {
                            EjecutarEliminacion(
                                connection,
                                transaction,
                                @"
DELETE FROM dbo.VisitaCliente
WHERE IdCliente = @Valor;

DELETE FROM dbo.Cliente
WHERE IdCliente = @Valor;",
                                idCliente);
                        }

                        if (datos.IdPersona > 0)
                        {
                            EjecutarEliminacion(
                                connection,
                                transaction,
                                @"
DELETE FROM dbo.ViajeParticipante
WHERE IdPersona = @Valor;

DELETE FROM dbo.Persona
WHERE IdPersona = @Valor;",
                                datos.IdPersona);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void EjecutarEliminacion(
            SqlConnection connection,
            SqlTransaction transaction,
            string sql,
            int valor)
        {
            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@Valor",
                    SqlDbType.Int).Value =
                        valor;

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

        private sealed class DatosPrueba
        {
            public DatosPrueba(
                DateTime fechaInicio,
                DateTime fechaFin,
                DateTime fechaVisita,
                string observacion,
                string localidadEncuentro)
            {
                FechaInicio = fechaInicio;
                FechaFin = fechaFin;
                FechaVisita = fechaVisita;
                Observacion = observacion;
                LocalidadEncuentro =
                    localidadEncuentro;
                IdClientes =
                    new List<int>();
            }

            public int IdPersona { get; set; }

            public int IdViaje { get; set; }

            public int IdVisita { get; set; }

            public DateTime FechaInicio { get; private set; }

            public DateTime FechaFin { get; private set; }

            public DateTime FechaVisita { get; private set; }

            public string Observacion { get; private set; }

            public string LocalidadEncuentro
            {
                get;
                private set;
            }

            public List<int> IdClientes
            {
                get;
                private set;
            }
        }
    }
}