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
using SIGEVIP.Infrastructure.Grupos;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class GrupoAuditoriaRepositoryIntegrationTests
    {
        [TestMethod]
        public void Insertar_ConAuditoriaValida_PersisteAsociacionesYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                datos.IdGrupoHijoOriginal =
                    InsertarGrupoDirecto(
                        datos.CodigoHijoOriginal,
                        datos.NombreHijoOriginal,
                        "Grupo hijo para alta.",
                        "CLIENTE_CONSULTAR");

                int idPermiso =
                    ObtenerIdPermiso(
                        "VIAJE_CONSULTAR");

                Grupo grupo =
                    new Grupo(
                        0,
                        datos.CodigoGrupoObjetivo,
                        datos.NombreGrupoObjetivo,
                        "Grupo creado con auditoría.");

                grupo.ReemplazarPermisosDirectos(
                    new[]
                    {
                        CrearPermisoDesdeBase(
                            idPermiso)
                    });

                grupo.ReemplazarGruposHijos(
                    new[]
                    {
                        CrearGrupoDesdeBase(
                            datos.IdGrupoHijoOriginal)
                    });

                GrupoGestionRepository repository =
                    CrearGrupoRepository();

                datos.IdGrupoObjetivo =
                    repository.Insertar(
                        grupo,
                        new[] { idPermiso },
                        new[]
                        {
                            datos.IdGrupoHijoOriginal
                        },
                        CrearAuditoria(
                            datos,
                            "Alta",
                            null,
                            datos.Marca +
                            " alta válida"));

                Grupo persistido =
                    repository.ObtenerPorId(
                        datos.IdGrupoObjetivo);

                Assert.IsNotNull(
                    persistido);

                CollectionAssert.AreEquivalent(
                    new[] { "VIAJE_CONSULTAR" },
                    persistido.Componentes
                        .OfType<Permiso>()
                        .Select(
                            permiso => permiso.Codigo)
                        .ToArray());

                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        datos.IdGrupoHijoOriginal
                    },
                    persistido.Componentes
                        .OfType<Grupo>()
                        .Select(
                            hijo => hijo.IdGrupo)
                        .ToArray());

                AuditoriaListadoDto evento =
                    BuscarEvento(
                        datos,
                        "Alta");

                Assert.IsNotNull(
                    evento);

                Assert.AreEqual(
                    datos.IdGrupoObjetivo,
                    evento.IdEntidad);

                Assert.AreEqual(
                    "Grupo",
                    evento.Entidad);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_ConActorInexistente_RevierteGrupoYAsociaciones()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                datos.IdGrupoHijoOriginal =
                    InsertarGrupoDirecto(
                        datos.CodigoHijoOriginal,
                        datos.NombreHijoOriginal,
                        "Grupo hijo para rollback.",
                        "CLIENTE_CONSULTAR");

                int idPermiso =
                    ObtenerIdPermiso(
                        "VIAJE_CONSULTAR");

                Grupo grupo =
                    new Grupo(
                        0,
                        datos.CodigoGrupoObjetivo,
                        datos.NombreGrupoObjetivo,
                        "Grupo que debe revertirse.");

                GrupoGestionRepository repository =
                    CrearGrupoRepository();

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Insertar(
                            grupo,
                            new[] { idPermiso },
                            new[]
                            {
                                datos.IdGrupoHijoOriginal
                            },
                            CrearAuditoriaInvalida(
                                datos,
                                "Alta",
                                null)));

                Assert.IsFalse(
                    repository.ExisteCodigo(
                        datos.CodigoGrupoObjetivo,
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
        public void Actualizar_ConAuditoriaValida_PersisteUnEventoConsolidado()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                CrearJerarquiaObjetivo(
                    datos);

                GrupoGestionRepository repository =
                    CrearGrupoRepository();

                Grupo grupo =
                    repository.ObtenerPorId(
                        datos.IdGrupoObjetivo);

                int idPermisoNuevo =
                    ObtenerIdPermiso(
                        "VIAJE_CONSULTAR");

                string nombreNuevo =
                    datos.NombreGrupoObjetivo +
                    " modificada";

                grupo.ActualizarDatos(
                    nombreNuevo,
                    "Descripción modificada.");

                grupo.ReemplazarPermisosDirectos(
                    new[]
                    {
                        CrearPermisoDesdeBase(
                            idPermisoNuevo)
                    });

                grupo.ReemplazarGruposHijos(
                    new[]
                    {
                        CrearGrupoDesdeBase(
                            datos.IdGrupoHijoNuevo)
                    });

                repository.Actualizar(
                    grupo,
                    new[] { idPermisoNuevo },
                    new[]
                    {
                        datos.IdGrupoHijoNuevo
                    },
                    CrearAuditoria(
                        datos,
                        "Modificacion",
                        datos.IdGrupoObjetivo,
                        datos.Marca +
                        " modificación consolidada"));

                Grupo persistido =
                    repository.ObtenerPorId(
                        datos.IdGrupoObjetivo);

                Assert.AreEqual(
                    nombreNuevo,
                    persistido.Nombre);

                CollectionAssert.AreEquivalent(
                    new[] { "VIAJE_CONSULTAR" },
                    persistido.Componentes
                        .OfType<Permiso>()
                        .Select(
                            permiso => permiso.Codigo)
                        .ToArray());

                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        datos.IdGrupoHijoNuevo
                    },
                    persistido.Componentes
                        .OfType<Grupo>()
                        .Select(
                            hijo => hijo.IdGrupo)
                        .ToArray());

                Assert.AreEqual(
                    1,
                    ContarAuditorias(
                        datos.Marca));

                AuditoriaListadoDto evento =
                    BuscarEvento(
                        datos,
                        "Modificacion");

                Assert.IsNotNull(
                    evento);

                Assert.AreEqual(
                    datos.IdGrupoObjetivo,
                    evento.IdEntidad);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_ConActorInexistente_RevierteDatosPermisosYJerarquia()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                CrearJerarquiaObjetivo(
                    datos);

                GrupoGestionRepository repository =
                    CrearGrupoRepository();

                Grupo grupo =
                    repository.ObtenerPorId(
                        datos.IdGrupoObjetivo);

                int idPermisoNuevo =
                    ObtenerIdPermiso(
                        "VIAJE_CONSULTAR");

                grupo.ActualizarDatos(
                    datos.NombreGrupoObjetivo +
                    " fallo",
                    "Cambio que debe revertirse.");

                grupo.ReemplazarPermisosDirectos(
                    new[]
                    {
                        CrearPermisoDesdeBase(
                            idPermisoNuevo)
                    });

                grupo.ReemplazarGruposHijos(
                    new[]
                    {
                        CrearGrupoDesdeBase(
                            datos.IdGrupoHijoNuevo)
                    });

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Actualizar(
                            grupo,
                            new[] { idPermisoNuevo },
                            new[]
                            {
                                datos.IdGrupoHijoNuevo
                            },
                            CrearAuditoriaInvalida(
                                datos,
                                "Modificacion",
                                datos.IdGrupoObjetivo)));

                Grupo persistido =
                    repository.ObtenerPorId(
                        datos.IdGrupoObjetivo);

                Assert.AreEqual(
                    datos.NombreGrupoObjetivo,
                    persistido.Nombre);

                CollectionAssert.AreEquivalent(
                    new[] { "CLIENTE_CONSULTAR" },
                    persistido.Componentes
                        .OfType<Permiso>()
                        .Select(
                            permiso => permiso.Codigo)
                        .ToArray());

                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        datos.IdGrupoHijoOriginal
                    },
                    persistido.Componentes
                        .OfType<Grupo>()
                        .Select(
                            hijo => hijo.IdGrupo)
                        .ToArray());

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
        public void CambiarEstado_ConAuditoriaValida_PersisteDosEventos()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                datos.IdGrupoObjetivo =
                    InsertarGrupoDirecto(
                        datos.CodigoGrupoObjetivo,
                        datos.NombreGrupoObjetivo,
                        "Grupo para cambio de estado.",
                        "CLIENTE_CONSULTAR");

                GrupoGestionRepository repository =
                    CrearGrupoRepository();

                repository.Desactivar(
                    datos.IdGrupoObjetivo,
                    CrearAuditoria(
                        datos,
                        "Desactivacion",
                        datos.IdGrupoObjetivo,
                        datos.Marca +
                        " desactivación"));

                repository.Activar(
                    datos.IdGrupoObjetivo,
                    CrearAuditoria(
                        datos,
                        "Activacion",
                        datos.IdGrupoObjetivo,
                        datos.Marca +
                        " activación"));

                Assert.IsTrue(
                    repository
                        .ObtenerPorId(
                            datos.IdGrupoObjetivo)
                        .Activo);

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

                Assert.IsTrue(
                    eventos.Any(
                        evento =>
                            evento.Accion ==
                            "Desactivacion"));

                Assert.IsTrue(
                    eventos.Any(
                        evento =>
                            evento.Accion ==
                            "Activacion"));
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
                datos.IdGrupoObjetivo =
                    InsertarGrupoDirecto(
                        datos.CodigoGrupoObjetivo,
                        datos.NombreGrupoObjetivo,
                        "Grupo para rollback de estado.",
                        "CLIENTE_CONSULTAR");

                GrupoGestionRepository repository =
                    CrearGrupoRepository();

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Desactivar(
                            datos.IdGrupoObjetivo,
                            CrearAuditoriaInvalida(
                                datos,
                                "Desactivacion",
                                datos.IdGrupoObjetivo)));

                Assert.IsTrue(
                    repository
                        .ObtenerPorId(
                            datos.IdGrupoObjetivo)
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

        private static GrupoGestionRepository
            CrearGrupoRepository()
        {
            return new GrupoGestionRepository(
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
                    "GRUPO_AUDIT_TEST_" +
                    sufijo,

                NombreActor =
                    "grupo_actor_" +
                    sufijo,

                EmailActor =
                    "grupo_actor_" +
                    sufijo +
                    "@sigevip.test",

                CodigoGrupoObjetivo =
                    "GRUPO_AUDIT_TEST_" +
                    sufijo +
                    "_OBJETIVO",

                NombreGrupoObjetivo =
                    "Grupo auditoría " +
                    sufijo,

                CodigoHijoOriginal =
                    "GRUPO_AUDIT_TEST_" +
                    sufijo +
                    "_HIJO_ORIGINAL",

                NombreHijoOriginal =
                    "Hijo original " +
                    sufijo,

                CodigoHijoNuevo =
                    "GRUPO_AUDIT_TEST_" +
                    sufijo +
                    "_HIJO_NUEVO",

                NombreHijoNuevo =
                    "Hijo nuevo " +
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
    N'Grupos',
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
    @PasswordHash,
    @PasswordSalt,
    1000,
    1
);

SELECT
    @IdPersona AS IdPersona,
    CAST(SCOPE_IDENTITY() AS INT) AS IdUsuario;";

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
                    "@PasswordHash",
                    SqlDbType.VarBinary,
                    32).Value =
                        CrearBytes(
                            1);

                command.Parameters.Add(
                    "@PasswordSalt",
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
                            reader.GetOrdinal(
                                "IdPersona"));

                    datos.IdUsuarioActor =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "IdUsuario"));
                }
            }
        }

        private static void CrearJerarquiaObjetivo(
            DatosPrueba datos)
        {
            datos.IdGrupoHijoOriginal =
                InsertarGrupoDirecto(
                    datos.CodigoHijoOriginal,
                    datos.NombreHijoOriginal,
                    "Grupo hijo original.",
                    "CLIENTE_CONSULTAR");

            datos.IdGrupoHijoNuevo =
                InsertarGrupoDirecto(
                    datos.CodigoHijoNuevo,
                    datos.NombreHijoNuevo,
                    "Grupo hijo nuevo.",
                    "CLIENTE_CONSULTAR");

            datos.IdGrupoObjetivo =
                InsertarGrupoDirecto(
                    datos.CodigoGrupoObjetivo,
                    datos.NombreGrupoObjetivo,
                    "Grupo objetivo original.",
                    "CLIENTE_CONSULTAR");

            const string sql = @"
INSERT INTO dbo.GrupoGrupo
(
    IdGrupoPadre,
    IdGrupoHijo
)
VALUES
(
    @IdGrupoPadre,
    @IdGrupoHijo
);";

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
                    "@IdGrupoPadre",
                    SqlDbType.Int).Value =
                        datos.IdGrupoObjetivo;

                command.Parameters.Add(
                    "@IdGrupoHijo",
                    SqlDbType.Int).Value =
                        datos.IdGrupoHijoOriginal;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static int InsertarGrupoDirecto(
            string codigo,
            string nombre,
            string descripcion,
            string codigoPermiso)
        {
            int idPermiso =
                ObtenerIdPermiso(
                    codigoPermiso);

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
    @Codigo,
    @Nombre,
    @Descripcion,
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
                    "@Codigo",
                    SqlDbType.NVarChar,
                    100).Value =
                        codigo;

                command.Parameters.Add(
                    "@Nombre",
                    SqlDbType.NVarChar,
                    150).Value =
                        nombre;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    500).Value =
                        descripcion;

                command.Parameters.Add(
                    "@IdPermiso",
                    SqlDbType.Int).Value =
                        idPermiso;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int ObtenerIdPermiso(
            string codigo)
        {
            const string sql = @"
SELECT IdPermiso
FROM dbo.Permiso
WHERE
    Codigo = @Codigo
    AND Activo = 1;";

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
                        codigo;

                connection.Open();

                object resultado =
                    command.ExecuteScalar();

                if (resultado == null ||
                    resultado == DBNull.Value)
                {
                    Assert.Fail(
                        "No existe el permiso activo " +
                        codigo +
                        ".");
                }

                return Convert.ToInt32(
                    resultado);
            }
        }

        private static Grupo CrearGrupoDesdeBase(
            int idGrupo)
        {
            const string sql = @"
SELECT
    IdGrupo,
    Codigo,
    Nombre,
    Descripcion,
    Activo
FROM dbo.Grupo
WHERE IdGrupo = @IdGrupo;";

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

                connection.Open();

                using (
                    SqlDataReader reader =
                        command.ExecuteReader(
                            CommandBehavior.SingleRow))
                {
                    Assert.IsTrue(
                        reader.Read());

                    Grupo grupo =
                        new Grupo(
                            reader.GetInt32(
                                reader.GetOrdinal(
                                    "IdGrupo")),
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Codigo")),
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Nombre")),
                            reader.IsDBNull(
                                reader.GetOrdinal(
                                    "Descripcion"))
                                ? string.Empty
                                : reader.GetString(
                                    reader.GetOrdinal(
                                        "Descripcion")));

                    if (!reader.GetBoolean(
                        reader.GetOrdinal(
                            "Activo")))
                    {
                        grupo.Desactivar();
                    }

                    return grupo;
                }
            }
        }

        private static Permiso CrearPermisoDesdeBase(
            int idPermiso)
        {
            const string sql = @"
SELECT
    IdPermiso,
    Codigo,
    Nombre,
    Descripcion,
    Activo
FROM dbo.Permiso
WHERE IdPermiso = @IdPermiso;";

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
                    "@IdPermiso",
                    SqlDbType.Int).Value =
                        idPermiso;

                connection.Open();

                using (
                    SqlDataReader reader =
                        command.ExecuteReader(
                            CommandBehavior.SingleRow))
                {
                    Assert.IsTrue(
                        reader.Read());

                    Permiso permiso =
                        new Permiso(
                            reader.GetInt32(
                                reader.GetOrdinal(
                                    "IdPermiso")),
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Codigo")),
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Nombre")),
                            reader.IsDBNull(
                                reader.GetOrdinal(
                                    "Descripcion"))
                                ? string.Empty
                                : reader.GetString(
                                    reader.GetOrdinal(
                                        "Descripcion")));

                    if (!reader.GetBoolean(
                        reader.GetOrdinal(
                            "Activo")))
                    {
                        permiso.Desactivar();
                    }

                    return permiso;
                }
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
                "Grupo",
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
                "actor.grupo.inexistente",
                "Seguridad",
                accion,
                "Grupo",
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

DELETE gg
FROM dbo.GrupoGrupo AS gg
WHERE
    EXISTS
    (
        SELECT 1
        FROM dbo.Grupo AS g
        WHERE
            g.IdGrupo = gg.IdGrupoPadre
            AND g.Codigo LIKE @CodigoGrupo
    )
    OR EXISTS
    (
        SELECT 1
        FROM dbo.Grupo AS g
        WHERE
            g.IdGrupo = gg.IdGrupoHijo
            AND g.Codigo LIKE @CodigoGrupo
    );

DELETE gp
FROM dbo.GrupoPermiso AS gp
INNER JOIN dbo.Grupo AS g
    ON g.IdGrupo = gp.IdGrupo
WHERE g.Codigo LIKE @CodigoGrupo;

DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario = @IdUsuarioActor;

DELETE FROM dbo.Usuario
WHERE IdUsuario = @IdUsuarioActor;

DELETE FROM dbo.Persona
WHERE IdPersona = @IdPersonaActor;

DELETE FROM dbo.Grupo
WHERE Codigo LIKE @CodigoGrupo;";

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
                    "@CodigoGrupo",
                    SqlDbType.NVarChar,
                    100).Value =
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

            public int IdGrupoObjetivo
            {
                get;
                set;
            }

            public string CodigoGrupoObjetivo
            {
                get;
                set;
            }

            public string NombreGrupoObjetivo
            {
                get;
                set;
            }

            public int IdGrupoHijoOriginal
            {
                get;
                set;
            }

            public string CodigoHijoOriginal
            {
                get;
                set;
            }

            public string NombreHijoOriginal
            {
                get;
                set;
            }

            public int IdGrupoHijoNuevo
            {
                get;
                set;
            }

            public string CodigoHijoNuevo
            {
                get;
                set;
            }

            public string NombreHijoNuevo
            {
                get;
                set;
            }
        }
    }
}