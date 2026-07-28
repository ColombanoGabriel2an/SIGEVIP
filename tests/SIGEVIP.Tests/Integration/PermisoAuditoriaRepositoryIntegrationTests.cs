using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Infrastructure.Auditoria;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Permisos;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class PermisoAuditoriaRepositoryIntegrationTests
    {
        [TestMethod]
        public void Insertar_ConAuditoriaValida_PersistePermisoYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                PermisoGestionRepository repository =
                    CrearRepository();

                Permiso permiso =
                    new Permiso(
                        0,
                        datos.CodigoPermiso,
                        datos.NombrePermiso,
                        "Permiso creado con auditoría.");

                datos.IdPermiso =
                    repository.Insertar(
                        permiso,
                        CrearAuditoria(
                            datos,
                            "Alta",
                            null,
                            datos.Marca +
                            " alta válida"));

                AuditoriaListadoDto evento =
                    BuscarEvento(
                        datos,
                        "Alta");

                Assert.IsNotNull(
                    evento);

                Assert.AreEqual(
                    datos.IdPermiso,
                    evento.IdEntidad);

                Assert.AreEqual(
                    "Permiso",
                    evento.Entidad);

                Assert.IsNotNull(
                    repository.ObtenerPorId(
                        datos.IdPermiso));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_ConActorInexistente_ReviertePermiso()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                Permiso permiso =
                    new Permiso(
                        0,
                        datos.CodigoPermiso,
                        datos.NombrePermiso,
                        "Permiso que debe revertirse.");

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Insertar(
                            permiso,
                            CrearAuditoriaInvalida(
                                datos,
                                "Alta",
                                null)));

                Assert.IsFalse(
                    repository.ExisteCodigo(
                        datos.CodigoPermiso,
                        null));

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
        public void Actualizar_ConAuditoriaValida_PersisteCambioYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                datos.IdPermiso =
                    InsertarPermisoDirecto(
                        datos,
                        true);

                PermisoGestionRepository repository =
                    CrearRepository();

                Permiso permiso =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                string nombreNuevo =
                    datos.NombrePermiso +
                    " modificado";

                permiso.ActualizarDatos(
                    nombreNuevo,
                    "Descripción modificada.");

                repository.Actualizar(
                    permiso,
                    CrearAuditoria(
                        datos,
                        "Modificacion",
                        datos.IdPermiso,
                        datos.Marca +
                        " modificación válida"));

                Permiso persistido =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                Assert.AreEqual(
                    nombreNuevo,
                    persistido.Nombre);

                Assert.AreEqual(
                    datos.CodigoPermiso,
                    persistido.Codigo);

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
        public void Actualizar_ConActorInexistente_RevierteCambio()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                datos.IdPermiso =
                    InsertarPermisoDirecto(
                        datos,
                        true);

                PermisoGestionRepository repository =
                    CrearRepository();

                Permiso permiso =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                string nombreOriginal =
                    permiso.Nombre;

                permiso.ActualizarDatos(
                    nombreOriginal +
                    " fallo",
                    "Cambio que debe revertirse.");

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Actualizar(
                            permiso,
                            CrearAuditoriaInvalida(
                                datos,
                                "Modificacion",
                                datos.IdPermiso)));

                Permiso persistido =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                Assert.AreEqual(
                    nombreOriginal,
                    persistido.Nombre);

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
        public void CambiarEstado_ConAuditoriaValida_ConservaAsociacionYRegistraEventos()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                datos.IdPermiso =
                    InsertarPermisoDirecto(
                        datos,
                        true);

                datos.IdGrupo =
                    InsertarGrupoAsociado(
                        datos);

                PermisoGestionRepository repository =
                    CrearRepository();

                repository.Desactivar(
                    datos.IdPermiso,
                    CrearAuditoria(
                        datos,
                        "Desactivacion",
                        datos.IdPermiso,
                        datos.Marca +
                        " desactivación"));

                Assert.IsTrue(
                    ExisteGrupoPermiso(
                        datos.IdGrupo,
                        datos.IdPermiso));

                repository.Activar(
                    datos.IdPermiso,
                    CrearAuditoria(
                        datos,
                        "Activacion",
                        datos.IdPermiso,
                        datos.Marca +
                        " activación"));

                Assert.IsTrue(
                    repository
                        .ObtenerPorId(
                            datos.IdPermiso)
                        .Activo);

                Assert.IsTrue(
                    ExisteGrupoPermiso(
                        datos.IdGrupo,
                        datos.IdPermiso));

                AuditoriaListadoDto[] eventos =
                    CrearAuditoriaRepository()
                        .Listar(
                            new AuditoriaFiltro(
                                null,
                                null,
                                datos.NombreActor,
                                "Seguridad",
                                string.Empty,
                                datos.Marca))
                        .ToArray();

                Assert.AreEqual(
                    2,
                    eventos.Length);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Desactivar_ConActorInexistente_RevierteEstado()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                datos.IdPermiso =
                    InsertarPermisoDirecto(
                        datos,
                        true);

                PermisoGestionRepository repository =
                    CrearRepository();

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Desactivar(
                            datos.IdPermiso,
                            CrearAuditoriaInvalida(
                                datos,
                                "Desactivacion",
                                datos.IdPermiso)));

                Assert.IsTrue(
                    repository
                        .ObtenerPorId(
                            datos.IdPermiso)
                        .Activo);

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

        private static PermisoGestionRepository CrearRepository()
        {
            return new PermisoGestionRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static AuditoriaRepository CrearAuditoriaRepository()
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
                    "PERMISO_AUDIT_TEST_" +
                    sufijo,

                NombreActor =
                    "permiso_actor_" +
                    sufijo,

                EmailActor =
                    "permiso_actor_" +
                    sufijo +
                    "@sigevip.test",

                CodigoPermiso =
                    (
                        "PERMISO_AUDIT_TEST_" +
                        sufijo
                    )
                    .ToUpperInvariant(),

                NombrePermiso =
                    "Permiso auditoría " +
                    sufijo,

                CodigoGrupo =
                    "GRUPO_PERMISO_AUDIT_" +
                    sufijo
            };
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
    N'Permisos',
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

        private static int InsertarPermisoDirecto(
            DatosPrueba datos,
            bool activo)
        {
            const string sql = @"
INSERT INTO dbo.Permiso
(
    Codigo,
    Nombre,
    Descripcion,
    Activo
)
VALUES
(
    @Codigo,
    @Nombre,
    @Descripcion,
    @Activo
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
                    "@Codigo",
                    SqlDbType.NVarChar,
                    100).Value =
                        datos.CodigoPermiso;

                command.Parameters.Add(
                    "@Nombre",
                    SqlDbType.NVarChar,
                    150).Value =
                        datos.NombrePermiso;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    500).Value =
                        "Permiso temporal de prueba.";

                command.Parameters.Add(
                    "@Activo",
                    SqlDbType.Bit).Value =
                        activo;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int InsertarGrupoAsociado(
            DatosPrueba datos)
        {
            const string sql = @"
INSERT INTO dbo.Grupo
(
    Codigo,
    Nombre,
    Descripcion,
    Activo
)
VALUES
(
    @CodigoGrupo,
    @NombreGrupo,
    N'Grupo temporal para auditoría de permisos.',
    1
);

DECLARE @IdGrupo INT =
    CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO dbo.GrupoPermiso
(
    IdGrupo,
    IdPermiso
)
VALUES
(
    @IdGrupo,
    @IdPermiso
);

SELECT @IdGrupo;";

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
                    "@CodigoGrupo",
                    SqlDbType.NVarChar,
                    100).Value =
                        datos.CodigoGrupo;

                command.Parameters.Add(
                    "@NombreGrupo",
                    SqlDbType.NVarChar,
                    150).Value =
                        "Grupo permiso auditoría";

                command.Parameters.Add(
                    "@IdPermiso",
                    SqlDbType.Int).Value =
                        datos.IdPermiso;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static bool ExisteGrupoPermiso(
            int idGrupo,
            int idPermiso)
        {
            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.GrupoPermiso
            WHERE
                IdGrupo = @IdGrupo
                AND IdPermiso = @IdPermiso
        )
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END;";

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
                    "@IdGrupo",
                    SqlDbType.Int).Value =
                        idGrupo;

                command.Parameters.Add(
                    "@IdPermiso",
                    SqlDbType.Int).Value =
                        idPermiso;

                connection.Open();

                return Convert.ToBoolean(
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
                "Seguridad",
                accion,
                "Permiso",
                idEntidad,
                descripcion);
        }

        private static AuditoriaRegistro CrearAuditoriaInvalida(
            DatosPrueba datos,
            string accion,
            int? idEntidad)
        {
            return new AuditoriaRegistro(
                int.MaxValue,
                "actor.permiso.inexistente",
                "Seguridad",
                accion,
                "Permiso",
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
                        "Seguridad",
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

DELETE FROM dbo.GrupoPermiso
WHERE
    IdPermiso = @IdPermiso
    OR IdGrupo = @IdGrupo;

DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario = @IdUsuarioActor;

DELETE FROM dbo.GrupoGrupo
WHERE
    IdGrupoPadre = @IdGrupo
    OR IdGrupoHijo = @IdGrupo;

DELETE FROM dbo.Grupo
WHERE IdGrupo = @IdGrupo;

DELETE FROM dbo.Permiso
WHERE IdPermiso = @IdPermiso;

DELETE FROM dbo.Usuario
WHERE IdUsuario = @IdUsuarioActor;

DELETE FROM dbo.Persona
WHERE IdPersona = @IdPersonaActor;";

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
                    "@IdPermiso",
                    SqlDbType.Int).Value =
                        datos.IdPermiso;

                command.Parameters.Add(
                    "@IdGrupo",
                    SqlDbType.Int).Value =
                        datos.IdGrupo;

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

            public int IdPermiso
            {
                get;
                set;
            }

            public string CodigoPermiso
            {
                get;
                set;
            }

            public string NombrePermiso
            {
                get;
                set;
            }

            public int IdGrupo
            {
                get;
                set;
            }

            public string CodigoGrupo
            {
                get;
                set;
            }
        }
    }
}