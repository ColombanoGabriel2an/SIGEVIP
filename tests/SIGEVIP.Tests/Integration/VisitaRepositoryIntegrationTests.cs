using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Visitas;
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
    public class VisitaRepositoryIntegrationTests
    {
        [TestMethod]
        public void ClienteConsulta_ListarActivos_DevuelveClienteActivo()
        {
            DatosVisitaPrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                ClienteSeleccionVisitaDto cliente =
                    CrearClienteRepository()
                        .ListarActivos()
                        .SingleOrDefault(
                            actual =>
                                actual.IdCliente ==
                                datos.IdClientes[0]);

                Assert.IsNotNull(
                    cliente);

                Assert.IsTrue(
                    cliente.Activo);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ClienteConsulta_ListarActivos_NoDevuelveClienteInactivo()
        {
            DatosVisitaPrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                DesactivarCliente(
                    datos.IdClientes[0]);

                bool encontrado =
                    CrearClienteRepository()
                        .ListarActivos()
                        .Any(
                            actual =>
                                actual.IdCliente ==
                                datos.IdClientes[0]);

                Assert.IsFalse(
                    encontrado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ClienteConsulta_ObtenerPorIds_ConservaClienteInactivo()
        {
            DatosVisitaPrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                DesactivarCliente(
                    datos.IdClientes[0]);

                Cliente cliente =
                    CrearClienteRepository()
                        .ObtenerPorIds(
                            new[]
                            {
                                datos.IdClientes[0]
                            })
                        .Single();

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
        public void Insertar_VisitaValida_PersisteVisitaYCliente()
        {
            DatosVisitaPrueba datos =
                CrearDatosPrueba(
                    1);

            try
            {
                Visita visita =
                    datos.CrearVisita(
                        new[]
                        {
                            0
                        });

                datos.IdVisita =
                    CrearVisitaRepository()
                        .Insertar(
                            visita);

                Assert.IsTrue(
                    datos.IdVisita > 0);

                Assert.AreEqual(
                    1,
                    ContarVisitas(
                        datos.IdVisita));

                Assert.AreEqual(
                    1,
                    ContarClientesDeVisita(
                        datos.IdVisita));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_ConVariosClientes_PersisteTodasLasAsociaciones()
        {
            DatosVisitaPrueba datos =
                CrearDatosPrueba(
                    2);

            try
            {
                datos.IdVisita =
                    CrearVisitaRepository()
                        .Insertar(
                            datos.CrearVisita(
                                new[]
                                {
                                    0,
                                    1
                                }));

                Assert.AreEqual(
                    2,
                    ContarClientesDeVisita(
                        datos.IdVisita));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_VisitaValida_ReemplazaDatosYClientes()
        {
            DatosVisitaPrueba datos =
                CrearDatosPrueba(
                    2);

            try
            {
                datos.IdVisita =
                    CrearVisitaRepository()
                        .Insertar(
                            datos.CrearVisita(
                                new[]
                                {
                                    0
                                }));

                Cliente segundoCliente =
                    CrearClienteRepository()
                        .ObtenerPorIds(
                            new[]
                            {
                                datos.IdClientes[1]
                            })
                        .Single();

                Visita modificada =
                    Visita.Reconstruir(
                        datos.IdVisita,
                        datos.IdViaje,
                        datos.FechaVisita.AddDays(1),
                        "Observación modificada",
                        "Funes",
                        new[]
                        {
                            segundoCliente
                        });

                CrearVisitaRepository()
                    .Actualizar(
                        modificada);

                VisitaListadoDto resultado =
                    CrearVisitaRepository()
                        .ListarPorViaje(
                            datos.IdViaje)
                        .Single(
                            actual =>
                                actual.IdVisita ==
                                    datos.IdVisita);

                Assert.AreEqual(
                    "Observación modificada",
                    resultado.Observacion);

                Assert.AreEqual(
                    "Funes",
                    resultado.LocalidadEncuentro);

                Assert.AreEqual(
                    1,
                    ContarClientesDeVisita(
                        datos.IdVisita));

                StringAssert.Contains(
                    resultado.ClientesResumen,
                    datos.RazonSocialBase +
                        "1");
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ListarPorViaje_DevuelveVisitaPersistida()
        {
            DatosVisitaPrueba datos =
                CrearEInsertarVisita();

            try
            {
                VisitaListadoDto resultado =
                    CrearVisitaRepository()
                        .ListarPorViaje(
                            datos.IdViaje)
                        .SingleOrDefault(
                            actual =>
                                actual.IdVisita ==
                                datos.IdVisita);

                Assert.IsNotNull(
                    resultado);

                Assert.AreEqual(
                    datos.Observacion,
                    resultado.Observacion);

                StringAssert.Contains(
                    resultado.ClientesResumen,
                    datos.RazonSocialBase);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ListarPorCliente_DevuelveVisitaPersistida()
        {
            DatosVisitaPrueba datos =
                CrearEInsertarVisita();

            try
            {
                VisitaListadoDto resultado =
                    CrearVisitaRepository()
                        .ListarPorCliente(
                            datos.IdClientes[0])
                        .SingleOrDefault(
                            actual =>
                                actual.IdVisita ==
                                datos.IdVisita);

                Assert.IsNotNull(
                    resultado);

                Assert.AreEqual(
                    datos.IdViaje,
                    resultado.IdViaje);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_ClienteInexistente_RevierteVisita()
        {
            DatosVisitaPrueba datos =
                CrearDatosPrueba(
                    1);

            string observacionRollback =
                datos.Observacion +
                " Rollback";

            try
            {
                Cliente inexistente =
                    new Cliente(
                        int.MaxValue,
                        "Cliente inexistente",
                        "30999999999",
                        string.Empty,
                        string.Empty,
                        "Rosario",
                        "Santa Fe");

                Visita visita =
                    new Visita(
                        0,
                        datos.FechaVisita,
                        observacionRollback,
                        datos.LocalidadEncuentro);

                visita.AgregarCliente(
                    inexistente);

                datos.CrearViajePersistido()
                    .AgregarVisita(
                        visita);

                Assert.ThrowsException<PersistenciaException>(
                    () => CrearVisitaRepository()
                        .Insertar(
                            visita));

                Assert.AreEqual(
                    0,
                    ContarVisitasPorObservacion(
                        observacionRollback));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
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

        private static ViajeRepository
            CrearViajeRepository()
        {
            return new ViajeRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosVisitaPrueba
            CrearEInsertarVisita()
        {
            DatosVisitaPrueba datos =
                CrearDatosPrueba(
                    1);

            datos.IdVisita =
                CrearVisitaRepository()
                    .Insertar(
                        datos.CrearVisita(
                            new[]
                            {
                                0
                            }));

            return datos;
        }

        private static DatosVisitaPrueba CrearDatosPrueba(
            int cantidadClientes)
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            var datos =
                new DatosVisitaPrueba(
                    "Visita integración " +
                        sufijo,
                    "Rosario",
                    "ClienteVisita" +
                        sufijo.Substring(
                            0,
                            8),
                    new DateTime(
                        2031,
                        6,
                        10),
                    new DateTime(
                        2031,
                        6,
                        15),
                    new DateTime(
                        2031,
                        6,
                        12));

            datos.IdPersona =
                InsertarPersona(
                    "Persona",
                    "Visita" +
                        sufijo.Substring(
                            0,
                            8),
                    "persona_" +
                        sufijo +
                        "@sigevip.test");

            datos.IdViaje =
                datos.CrearViajePersistido()
                    .IdViaje;

            for (
                int indice = 0;
                indice < cantidadClientes;
                indice++)
            {
                datos.IdClientes.Add(
                    InsertarCliente(
                        datos.RazonSocialBase +
                            indice,
                        CrearCuit(
                            sufijo,
                            indice)));
            }

            return datos;
        }

        private static string CrearCuit(
            string sufijo,
            int indice)
        {
            if (indice < 0 ||
                indice > 99)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(indice),
                    "El índice del Cliente debe encontrarse entre 0 y 99.");
            }

            int baseNumerica =
                Convert.ToInt32(
                    sufijo.Substring(
                        0,
                        7),
                    16)
                % 10000000;

            return
                "30" +
                baseNumerica.ToString(
                    "D7") +
                indice.ToString(
                    "D2");
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
    Localidad,
    Provincia,
    Activo
)
VALUES
(
    @RazonSocial,
    @Cuit,
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

        private static int ContarVisitas(
            int idVisita)
        {
            return EjecutarConteo(
                @"
SELECT COUNT(*)
FROM dbo.Visita
WHERE IdVisita = @Valor;",
                idVisita);
        }

        private static int ContarClientesDeVisita(
            int idVisita)
        {
            return EjecutarConteo(
                @"
SELECT COUNT(*)
FROM dbo.VisitaCliente
WHERE IdVisita = @Valor;",
                idVisita);
        }

        private static int ContarVisitasPorObservacion(
            string observacion)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Visita
WHERE Observacion = @Observacion;";

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
                    "@Observacion",
                    SqlDbType.NVarChar,
                    1000).Value =
                        observacion;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int EjecutarConteo(
            string sql,
            int valor)
        {
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
                    "@Valor",
                    SqlDbType.Int).Value =
                        valor;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static void EliminarDatosPrueba(
            DatosVisitaPrueba datos)
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
                    .ConnectionStrings["SIGEVIP"];

            if (settings == null ||
                string.IsNullOrWhiteSpace(
                    settings.ConnectionString))
            {
                Assert.Fail(
                    "No se encontró la cadena SIGEVIP en App.config.");
            }

            return settings.ConnectionString;
        }

        private sealed class DatosVisitaPrueba
        {
            public DatosVisitaPrueba(
                string observacion,
                string localidadEncuentro,
                string razonSocialBase,
                DateTime fechaInicio,
                DateTime fechaFin,
                DateTime fechaVisita)
            {
                Observacion = observacion;
                LocalidadEncuentro =
                    localidadEncuentro;
                RazonSocialBase =
                    razonSocialBase;
                FechaInicio = fechaInicio;
                FechaFin = fechaFin;
                FechaVisita = fechaVisita;
                IdClientes =
                    new List<int>();
            }

            public int IdPersona { get; set; }

            public int IdViaje { get; set; }

            public int IdVisita { get; set; }

            public string Observacion { get; private set; }

            public string LocalidadEncuentro
            {
                get;
                private set;
            }

            public string RazonSocialBase { get; private set; }

            public DateTime FechaInicio { get; private set; }

            public DateTime FechaFin { get; private set; }

            public DateTime FechaVisita { get; private set; }

            public List<int> IdClientes
            {
                get;
                private set;
            }

            public Viaje CrearViajePersistido()
            {
                if (IdViaje > 0)
                {
                    return CrearViajeRepository()
                        .ObtenerPorId(
                            IdViaje);
                }

                Persona participante =
                    new Persona(
                        IdPersona,
                        "Persona",
                        "Visita",
                        "persona.visita@sigevip.test");

                var viaje =
                    new Viaje(
                        0,
                        FechaInicio,
                        FechaFin,
                        "Viaje para " +
                            Observacion,
                        TipoViaje.Desplazamiento,
                        0m);

                viaje.ReemplazarParticipantes(
                    new[]
                    {
                        participante
                    });

                IdViaje =
                    CrearViajeRepository()
                        .Insertar(
                            viaje);

                return CrearViajeRepository()
                    .ObtenerPorId(
                        IdViaje);
            }

            public Visita CrearVisita(
                IEnumerable<int> indicesClientes)
            {
                var visita =
                    new Visita(
                        0,
                        FechaVisita,
                        Observacion,
                        LocalidadEncuentro);

                IReadOnlyCollection<Cliente> clientes =
                    CrearClienteRepository()
                        .ObtenerPorIds(
                            indicesClientes
                                .Select(
                                    indice =>
                                        IdClientes[
                                            indice])
                                .ToList()
                                .AsReadOnly());

                foreach (
                    Cliente cliente
                    in clientes)
                {
                    visita.AgregarCliente(
                        cliente);
                }

                CrearViajePersistido()
                    .AgregarVisita(
                        visita);

                return visita;
            }
        }
    }
}