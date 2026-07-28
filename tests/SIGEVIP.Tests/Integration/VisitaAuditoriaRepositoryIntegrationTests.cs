using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Viajes;
using SIGEVIP.Infrastructure.Visitas;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class VisitaAuditoriaRepositoryIntegrationTests
    {
        [TestMethod]
        public void Insertar_ConAuditoriaValida_PersisteVisitaClientesYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                PrepararContexto(
                    datos,
                    true);

                Visita visita =
                    CrearVisita(
                        datos);

                datos.IdVisita =
                    CrearVisitaRepository()
                        .Insertar(
                            visita,
                            CrearAuditoriaValida(
                                datos));

                Assert.IsTrue(
                    datos.IdVisita > 0);

                Assert.AreEqual(
                    1,
                    ContarVisitasPorId(
                        datos.IdVisita));

                Assert.AreEqual(
                    1,
                    ContarClientesDeVisita(
                        datos.IdVisita));

                Assert.AreEqual(
                    1,
                    ContarAuditorias(
                        datos));

                Assert.AreEqual(
                    datos.IdVisita,
                    ObtenerIdEntidadAuditoria(
                        datos));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_ConActorInexistente_RevierteVisitaYClientes()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                PrepararContexto(
                    datos,
                    false);

                Visita visita =
                    CrearVisita(
                        datos);

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => CrearVisitaRepository()
                            .Insertar(
                                visita,
                                CrearAuditoriaInvalida(
                                    datos)));

                Assert.AreEqual(
                    0,
                    ContarVisitasPorObservacion(
                        datos.Observacion));

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static DatosPrueba CrearDatosPrueba()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(
                        0,
                        12);

            return new DatosPrueba
            {
                Marca =
                    "VISITA_AUDIT_TEST_" +
                    sufijo,

                NombreActor =
                    "visita_actor_" +
                    sufijo,

                EmailActor =
                    "visita_actor_" +
                    sufijo +
                    "@sigevip.test",

                EmailParticipante =
                    "visita_participante_" +
                    sufijo +
                    "@sigevip.test",

                RazonSocialCliente =
                    "Cliente visita audit " +
                    sufijo,

                CuitCliente =
                    CrearCuit(
                        sufijo),

                Observacion =
                    "VISITA_AUDIT_TEST_" +
                    sufijo +
                    " observacion",

                FechaInicio =
                    new DateTime(
                        2036,
                        7,
                        10),

                FechaFin =
                    new DateTime(
                        2036,
                        7,
                        15),

                FechaVisita =
                    new DateTime(
                        2036,
                        7,
                        12)
            };
        }

        private static void PrepararContexto(
            DatosPrueba datos,
            bool insertarActor)
        {
            if (insertarActor)
            {
                InsertarActor(
                    datos);
            }

            datos.IdPersonaParticipante =
                InsertarPersona(
                    "Participante",
                    "Visita",
                    datos.EmailParticipante);

            Persona participante =
                new Persona(
                    datos.IdPersonaParticipante,
                    "Participante",
                    "Visita",
                    datos.EmailParticipante);

            Viaje viaje =
                new Viaje(
                    0,
                    datos.FechaInicio,
                    datos.FechaFin,
                    "Viaje para " +
                        datos.Marca,
                    TipoViaje.Desplazamiento,
                    0m);

            viaje.ReemplazarParticipantes(
                new[]
                {
                    participante
                });

            datos.IdViaje =
                CrearViajeRepository()
                    .Insertar(
                        viaje);

            datos.IdCliente =
                InsertarCliente(
                    datos.RazonSocialCliente,
                    datos.CuitCliente);
        }

        private static Visita CrearVisita(
            DatosPrueba datos)
        {
            Cliente cliente =
                new Cliente(
                    datos.IdCliente,
                    datos.RazonSocialCliente,
                    datos.CuitCliente,
                    string.Empty,
                    string.Empty,
                    "Rosario",
                    "Santa Fe");

            Visita visita =
                new Visita(
                    0,
                    datos.FechaVisita,
                    datos.Observacion,
                    "Rosario");

            visita.AgregarCliente(
                cliente);

            Viaje viaje =
                CrearViajeRepository()
                    .ObtenerPorId(
                        datos.IdViaje);

            viaje.AgregarVisita(
                visita);

            return visita;
        }

        private static AuditoriaRegistro
            CrearAuditoriaValida(
                DatosPrueba datos)
        {
            return new AuditoriaRegistro(
                datos.IdUsuarioActor,
                datos.NombreActor,
                "Viajes",
                "Alta",
                "Visita",
                null,
                datos.Marca +
                " alta valida");
        }

        private static AuditoriaRegistro
            CrearAuditoriaInvalida(
                DatosPrueba datos)
        {
            return new AuditoriaRegistro(
                int.MaxValue,
                "actor.visita.inexistente",
                "Viajes",
                "Alta",
                "Visita",
                null,
                datos.Marca +
                " auditoria invalida");
        }

        private static VisitaRepository
            CrearVisitaRepository()
        {
            return new VisitaRepository(
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

        private static void InsertarActor(
            DatosPrueba datos)
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
    N'Auditor',
    N'Visitas',
    @Email,
    1
);

DECLARE @IdPersona INT =
    CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO dbo.Usuario
(
    IdPersona,
    NombreUsuario,
    PasswordHash,
    PasswordSalt,
    IteracionesPassword,
    Activo
)
VALUES
(
    @IdPersona,
    @NombreUsuario,
    @Hash,
    @Salt,
    100000,
    1
);

SELECT
    @IdPersona,
    CAST(SCOPE_IDENTITY() AS INT);";

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
                    "@Email",
                    SqlDbType.NVarChar,
                    254).Value =
                        datos.EmailActor;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        datos.NombreActor;

                command.Parameters.Add(
                    "@Hash",
                    SqlDbType.VarBinary,
                    32).Value =
                        CrearBytes(
                            1);

                command.Parameters.Add(
                    "@Salt",
                    SqlDbType.VarBinary,
                    32).Value =
                        CrearBytes(
                            2);

                connection.Open();

                using (
                    SqlDataReader reader =
                        command.ExecuteReader(
                            CommandBehavior.SingleRow))
                {
                    Assert.IsTrue(
                        reader.Read());

                    datos.IdPersonaActor =
                        reader.GetInt32(
                            0);

                    datos.IdUsuarioActor =
                        reader.GetInt32(
                            1);
                }
            }
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

        private static int ContarVisitasPorId(
            int idVisita)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Visita
WHERE IdVisita = @IdVisita;";

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
                    "@IdVisita",
                    SqlDbType.Int).Value =
                        idVisita;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int ContarClientesDeVisita(
            int idVisita)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.VisitaCliente
WHERE IdVisita = @IdVisita;";

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
                    "@IdVisita",
                    SqlDbType.Int).Value =
                        idVisita;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
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

        private static int ContarAuditorias(
            DatosPrueba datos)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Auditoria
WHERE
    Descripcion LIKE @Marca
    AND Modulo = N'Viajes'
    AND Accion = N'Alta'
    AND Entidad = N'Visita';";

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
                    "@Marca",
                    SqlDbType.NVarChar,
                    1000).Value =
                        "%" +
                        datos.Marca +
                        "%";

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int ObtenerIdEntidadAuditoria(
            DatosPrueba datos)
        {
            const string sql = @"
SELECT TOP (1)
    IdEntidad
FROM dbo.Auditoria
WHERE
    Descripcion LIKE @Marca
    AND Modulo = N'Viajes'
    AND Accion = N'Alta'
    AND Entidad = N'Visita'
ORDER BY IdAuditoria DESC;";

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
                    "@Marca",
                    SqlDbType.NVarChar,
                    1000).Value =
                        "%" +
                        datos.Marca +
                        "%";

                connection.Open();

                object resultado =
                    command.ExecuteScalar();

                Assert.IsNotNull(
                    resultado);

                Assert.AreNotEqual(
                    DBNull.Value,
                    resultado);

                return Convert.ToInt32(
                    resultado);
            }
        }

        private static void EliminarDatosPrueba(
            DatosPrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            const string sql = @"
DELETE FROM dbo.Auditoria
WHERE
    IdUsuario = @IdUsuarioActor
    OR NombreUsuario = @NombreActor
    OR Descripcion LIKE @Marca;

DELETE relacion
FROM dbo.VisitaCliente AS relacion
INNER JOIN dbo.Visita AS visita
    ON visita.IdVisita =
        relacion.IdVisita
WHERE
    visita.IdViaje = @IdViaje
    OR visita.Observacion LIKE @Marca;

DELETE FROM dbo.Visita
WHERE
    IdViaje = @IdViaje
    OR Observacion LIKE @Marca;

DELETE FROM dbo.ViajeParticipante
WHERE IdViaje = @IdViaje;

DELETE FROM dbo.Viaje
WHERE IdViaje = @IdViaje;

DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario = @IdUsuarioActor;

DELETE FROM dbo.Usuario
WHERE IdUsuario = @IdUsuarioActor;

DELETE FROM dbo.Cliente
WHERE
    IdCliente = @IdCliente
    OR RazonSocial LIKE @MarcaCliente;

DELETE FROM dbo.Persona
WHERE
    IdPersona IN
    (
        @IdPersonaActor,
        @IdPersonaParticipante
    )
    OR Email = @EmailActor
    OR Email = @EmailParticipante;";

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
                        using (
                            SqlCommand command =
                                new SqlCommand(
                                    sql,
                                    connection,
                                    transaction))
                        {
                            command.Parameters.Add(
                                "@IdUsuarioActor",
                                SqlDbType.Int).Value =
                                    datos.IdUsuarioActor;

                            command.Parameters.Add(
                                "@IdPersonaActor",
                                SqlDbType.Int).Value =
                                    datos.IdPersonaActor;

                            command.Parameters.Add(
                                "@IdPersonaParticipante",
                                SqlDbType.Int).Value =
                                    datos.IdPersonaParticipante;

                            command.Parameters.Add(
                                "@IdViaje",
                                SqlDbType.Int).Value =
                                    datos.IdViaje;

                            command.Parameters.Add(
                                "@IdCliente",
                                SqlDbType.Int).Value =
                                    datos.IdCliente;

                            command.Parameters.Add(
                                "@NombreActor",
                                SqlDbType.NVarChar,
                                100).Value =
                                    datos.NombreActor
                                    ?? string.Empty;

                            command.Parameters.Add(
                                "@Marca",
                                SqlDbType.NVarChar,
                                1000).Value =
                                    "%" +
                                    (
                                        datos.Marca
                                        ?? string.Empty
                                    ) +
                                    "%";

                            command.Parameters.Add(
                                "@MarcaCliente",
                                SqlDbType.NVarChar,
                                150).Value =
                                    "%Cliente visita audit%";

                            command.Parameters.Add(
                                "@EmailActor",
                                SqlDbType.NVarChar,
                                254).Value =
                                    datos.EmailActor
                                    ?? string.Empty;

                            command.Parameters.Add(
                                "@EmailParticipante",
                                SqlDbType.NVarChar,
                                254).Value =
                                    datos.EmailParticipante
                                    ?? string.Empty;

                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        if (transaction.Connection != null)
                        {
                            transaction.Rollback();
                        }

                        throw;
                    }
                }
            }
        }

        private static string CrearCuit(
            string sufijo)
        {
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
                "00";
        }

        private static byte[] CrearBytes(
            byte valor)
        {
            byte[] resultado =
                new byte[32];

            for (
                int indice = 0;
                indice < resultado.Length;
                indice++)
            {
                resultado[indice] =
                    valor;
            }

            return resultado;
        }

        private static string ObtenerConnectionString()
        {
            ConnectionStringSettings settings =
                ConfigurationManager
                    .ConnectionStrings[
                        "SIGEVIP"];

            Assert.IsNotNull(
                settings);

            Assert.IsFalse(
                string.IsNullOrWhiteSpace(
                    settings.ConnectionString));

            return settings.ConnectionString;
        }

        private sealed class DatosPrueba
        {
            public string Marca
            {
                get;
                set;
            }

            public int IdPersonaActor
            {
                get;
                set;
            }

            public int IdUsuarioActor
            {
                get;
                set;
            }

            public string NombreActor
            {
                get;
                set;
            }

            public string EmailActor
            {
                get;
                set;
            }

            public int IdPersonaParticipante
            {
                get;
                set;
            }

            public string EmailParticipante
            {
                get;
                set;
            }

            public int IdViaje
            {
                get;
                set;
            }

            public int IdCliente
            {
                get;
                set;
            }

            public string RazonSocialCliente
            {
                get;
                set;
            }

            public string CuitCliente
            {
                get;
                set;
            }

            public int IdVisita
            {
                get;
                set;
            }

            public string Observacion
            {
                get;
                set;
            }

            public DateTime FechaInicio
            {
                get;
                set;
            }

            public DateTime FechaFin
            {
                get;
                set;
            }

            public DateTime FechaVisita
            {
                get;
                set;
            }
        }
    }
}