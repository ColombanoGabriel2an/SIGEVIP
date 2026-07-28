using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Infrastructure.Auditoria;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Viajes;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class ViajeAuditoriaRepositoryIntegrationTests
    {
        [TestMethod]
        public void Insertar_ConAuditoriaValida_PersisteViajeParticipantesYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                datos.IdPersonaOriginal =
                    InsertarPersona(
                        datos.EmailPersonaOriginal,
                        "Original");

                Viaje viaje =
                    CrearViaje(
                        datos,
                        datos.DescripcionOriginal,
                        CrearPersona(
                            datos.IdPersonaOriginal,
                            datos.EmailPersonaOriginal,
                            "Original"));

                ViajeRepository repository =
                    CrearViajeRepository();

                datos.IdViaje =
                    repository.Insertar(
                        viaje,
                        CrearAuditoria(
                            datos,
                            "Alta",
                            null,
                            datos.Marca +
                            " alta válida"));

                Viaje persistido =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.IsNotNull(
                    persistido);

                Assert.AreEqual(
                    1,
                    persistido.Participantes.Count);

                Assert.AreEqual(
                    datos.IdPersonaOriginal,
                    persistido
                        .Participantes
                        .Single()
                        .IdPersona);

                AuditoriaListadoDto evento =
                    BuscarEvento(
                        datos,
                        "Alta");

                Assert.IsNotNull(
                    evento);

                Assert.AreEqual(
                    datos.IdViaje,
                    evento.IdEntidad);

                Assert.AreEqual(
                    "Viaje",
                    evento.Entidad);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_ConActorInexistente_RevierteViajeYParticipantes()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                datos.IdPersonaOriginal =
                    InsertarPersona(
                        datos.EmailPersonaOriginal,
                        "Original");

                Viaje viaje =
                    CrearViaje(
                        datos,
                        datos.DescripcionOriginal,
                        CrearPersona(
                            datos.IdPersonaOriginal,
                            datos.EmailPersonaOriginal,
                            "Original"));

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => CrearViajeRepository()
                            .Insertar(
                                viaje,
                                CrearAuditoriaInvalida(
                                    datos,
                                    "Alta",
                                    null)));

                Assert.AreEqual(
                    0,
                    ContarViajes(
                        datos.Marca));

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos.Marca));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_ConAuditoriaValida_PersisteDatosParticipantesYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                PrepararViajeConDosPersonas(
                    datos);

                ViajeRepository repository =
                    CrearViajeRepository();

                Viaje viaje =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                viaje.ActualizarDatos(
                    datos.FechaInicio.AddDays(
                        1),
                    datos.FechaFin.AddDays(
                        2),
                    datos.DescripcionModificada,
                    TipoViaje.EventoFeria,
                    2750m,
                    new[]
                    {
                        CrearPersona(
                            datos.IdPersonaNueva,
                            datos.EmailPersonaNueva,
                            "Nueva")
                    });

                repository.Actualizar(
                    viaje,
                    CrearAuditoria(
                        datos,
                        "Modificacion",
                        datos.IdViaje,
                        datos.Marca +
                        " modificación válida"));

                Viaje persistido =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    datos.DescripcionModificada,
                    persistido.Descripcion);

                Assert.AreEqual(
                    TipoViaje.EventoFeria,
                    persistido.TipoViaje);

                Assert.AreEqual(
                    2750m,
                    persistido.MontoAnticipado);

                Assert.AreEqual(
                    datos.IdPersonaNueva,
                    persistido
                        .Participantes
                        .Single()
                        .IdPersona);

                Assert.AreEqual(
                    1,
                    ContarAuditorias(
                        datos.Marca));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_ConActorInexistente_RevierteDatosYParticipantes()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                PrepararViajeConDosPersonas(
                    datos);

                ViajeRepository repository =
                    CrearViajeRepository();

                Viaje viaje =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                viaje.ActualizarDatos(
                    datos.FechaInicio.AddDays(
                        1),
                    datos.FechaFin.AddDays(
                        2),
                    datos.DescripcionModificada,
                    TipoViaje.EventoFeria,
                    2750m,
                    new[]
                    {
                        CrearPersona(
                            datos.IdPersonaNueva,
                            datos.EmailPersonaNueva,
                            "Nueva")
                    });

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Actualizar(
                            viaje,
                            CrearAuditoriaInvalida(
                                datos,
                                "Modificacion",
                                datos.IdViaje)));

                Viaje persistido =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    datos.DescripcionOriginal,
                    persistido.Descripcion);

                Assert.AreEqual(
                    TipoViaje.Desplazamiento,
                    persistido.TipoViaje);

                Assert.AreEqual(
                    datos.IdPersonaOriginal,
                    persistido
                        .Participantes
                        .Single()
                        .IdPersona);

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos.Marca));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Cancelar_ConAuditoriaValida_PersisteEstadoYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                PrepararViajeConUnaPersona(
                    datos);

                ViajeRepository repository =
                    CrearViajeRepository();

                Viaje viaje =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                viaje.Cancelar();

                repository.Cancelar(
                    viaje,
                    CrearAuditoria(
                        datos,
                        "Cancelacion",
                        datos.IdViaje,
                        datos.Marca +
                        " cancelación válida"));

                Assert.AreEqual(
                    EstadoViaje.Cancelado,
                    repository
                        .ObtenerPorId(
                            datos.IdViaje)
                        .EstadoActual);

                AuditoriaListadoDto evento =
                    BuscarEvento(
                        datos,
                        "Cancelacion");

                Assert.IsNotNull(
                    evento);

                Assert.AreEqual(
                    datos.IdViaje,
                    evento.IdEntidad);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Cancelar_ConActorInexistente_RevierteEstado()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                PrepararViajeConUnaPersona(
                    datos);

                ViajeRepository repository =
                    CrearViajeRepository();

                Viaje viaje =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                viaje.Cancelar();

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Cancelar(
                            viaje,
                            CrearAuditoriaInvalida(
                                datos,
                                "Cancelacion",
                                datos.IdViaje)));

                Assert.AreEqual(
                    EstadoViaje.Abierto,
                    repository
                        .ObtenerPorId(
                            datos.IdViaje)
                        .EstadoActual);

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos.Marca));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static ViajeRepository
            CrearViajeRepository()
        {
            return new ViajeRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static AuditoriaRepository
            CrearAuditoriaRepository()
        {
            return new AuditoriaRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
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
                    "VIAJE_AUDIT_TEST_" +
                    sufijo,

                NombreActor =
                    "viaje_actor_" +
                    sufijo,

                EmailActor =
                    "viaje_actor_" +
                    sufijo +
                    "@sigevip.test",

                EmailPersonaOriginal =
                    "viaje_audit_original_" +
                    sufijo +
                    "@sigevip.test",

                EmailPersonaNueva =
                    "viaje_audit_nueva_" +
                    sufijo +
                    "@sigevip.test",

                DescripcionOriginal =
                    "VIAJE_AUDIT_TEST_" +
                    sufijo +
                    " original",

                DescripcionModificada =
                    "VIAJE_AUDIT_TEST_" +
                    sufijo +
                    " modificada",

                FechaInicio =
                    new DateTime(
                        2035,
                        5,
                        10),

                FechaFin =
                    new DateTime(
                        2035,
                        5,
                        15)
            };
        }

        private static void PrepararViajeConUnaPersona(
            DatosPrueba datos)
        {
            datos.IdPersonaOriginal =
                InsertarPersona(
                    datos.EmailPersonaOriginal,
                    "Original");

            Viaje viaje =
                CrearViaje(
                    datos,
                    datos.DescripcionOriginal,
                    CrearPersona(
                        datos.IdPersonaOriginal,
                        datos.EmailPersonaOriginal,
                        "Original"));

            datos.IdViaje =
                CrearViajeRepository()
                    .Insertar(
                        viaje);
        }

        private static void PrepararViajeConDosPersonas(
            DatosPrueba datos)
        {
            datos.IdPersonaOriginal =
                InsertarPersona(
                    datos.EmailPersonaOriginal,
                    "Original");

            datos.IdPersonaNueva =
                InsertarPersona(
                    datos.EmailPersonaNueva,
                    "Nueva");

            Viaje viaje =
                CrearViaje(
                    datos,
                    datos.DescripcionOriginal,
                    CrearPersona(
                        datos.IdPersonaOriginal,
                        datos.EmailPersonaOriginal,
                        "Original"));

            datos.IdViaje =
                CrearViajeRepository()
                    .Insertar(
                        viaje);
        }

        private static Viaje CrearViaje(
            DatosPrueba datos,
            string descripcion,
            params Persona[] participantes)
        {
            Viaje viaje =
                new Viaje(
                    0,
                    datos.FechaInicio,
                    datos.FechaFin,
                    descripcion,
                    TipoViaje.Desplazamiento,
                    1500m);

            viaje.ReemplazarParticipantes(
                participantes);

            return viaje;
        }

        private static Persona CrearPersona(
            int idPersona,
            string email,
            string apellido)
        {
            return new Persona(
                idPersona,
                "Persona",
                apellido,
                email);
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
    N'Viajes',
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
    1000,
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
            string email,
            string apellido)
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
    N'Persona',
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

        private static AuditoriaRegistro CrearAuditoria(
            DatosPrueba datos,
            string accion,
            int? idEntidad,
            string descripcion)
        {
            return new AuditoriaRegistro(
                datos.IdUsuarioActor,
                datos.NombreActor,
                "Viajes",
                accion,
                "Viaje",
                idEntidad,
                descripcion);
        }

        private static AuditoriaRegistro
            CrearAuditoriaInvalida(
                DatosPrueba datos,
                string accion,
                int? idEntidad)
        {
            return new AuditoriaRegistro(
                int.MaxValue,
                "actor.viaje.inexistente",
                "Viajes",
                accion,
                "Viaje",
                idEntidad,
                datos.Marca +
                " auditoría inválida");
        }

        private static AuditoriaListadoDto BuscarEvento(
            DatosPrueba datos,
            string accion)
        {
            return CrearAuditoriaRepository()
                .Listar(
                    new AuditoriaFiltro(
                        null,
                        null,
                        datos.NombreActor,
                        "Viajes",
                        accion,
                        datos.Marca))
                .SingleOrDefault();
        }

        private static int ContarAuditorias(
            string marca)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Auditoria
WHERE Descripcion LIKE @Marca;";

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
                        marca +
                        "%";

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int ContarViajes(
            string marca)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Viaje
WHERE Descripcion LIKE @Marca;";

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
                    500).Value =
                        "%" +
                        marca +
                        "%";

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
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

DELETE vp
FROM dbo.ViajeParticipante AS vp
INNER JOIN dbo.Viaje AS viaje
    ON viaje.IdViaje = vp.IdViaje
WHERE
    viaje.IdViaje = @IdViaje
    OR viaje.Descripcion LIKE @Marca;

DELETE FROM dbo.Viaje
WHERE
    IdViaje = @IdViaje
    OR Descripcion LIKE @Marca;

DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario = @IdUsuarioActor;

DELETE FROM dbo.Usuario
WHERE IdUsuario = @IdUsuarioActor;

DELETE FROM dbo.Persona
WHERE
    IdPersona IN
    (
        @IdPersonaActor,
        @IdPersonaOriginal,
        @IdPersonaNueva
    )
    OR Email LIKE @EmailTemporal;";

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
                    "@IdUsuarioActor",
                    SqlDbType.Int).Value =
                        datos.IdUsuarioActor;

                command.Parameters.Add(
                    "@IdPersonaActor",
                    SqlDbType.Int).Value =
                        datos.IdPersonaActor;

                command.Parameters.Add(
                    "@IdPersonaOriginal",
                    SqlDbType.Int).Value =
                        datos.IdPersonaOriginal;

                command.Parameters.Add(
                    "@IdPersonaNueva",
                    SqlDbType.Int).Value =
                        datos.IdPersonaNueva;

                command.Parameters.Add(
                    "@IdViaje",
                    SqlDbType.Int).Value =
                        datos.IdViaje;

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
                    "@EmailTemporal",
                    SqlDbType.NVarChar,
                    254).Value =
                        "%viaje_audit_%@sigevip.test";

                connection.Open();

                command.ExecuteNonQuery();
            }
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

            public int IdViaje
            {
                get;
                set;
            }

            public int IdPersonaOriginal
            {
                get;
                set;
            }

            public int IdPersonaNueva
            {
                get;
                set;
            }

            public string EmailPersonaOriginal
            {
                get;
                set;
            }

            public string EmailPersonaNueva
            {
                get;
                set;
            }

            public string DescripcionOriginal
            {
                get;
                set;
            }

            public string DescripcionModificada
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
        }
    }
}