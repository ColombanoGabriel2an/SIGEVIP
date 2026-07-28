using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Infrastructure.Auditoria;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Security;
using SIGEVIP.Infrastructure.Usuarios;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class UsuarioAuditoriaRepositoryIntegrationTests
    {
        [TestMethod]
        public void Insertar_ConAuditoriaValida_PersisteUsuarioYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                datos.IdPersonaObjetivo =
                    InsertarPersona(
                        datos.EmailObjetivo);

                int idGrupo =
                    ObtenerIdGrupo(
                        "COMERCIAL");

                Usuario usuario =
                    CrearUsuarioNuevo(
                        datos.IdPersonaObjetivo,
                        datos.NombreUsuarioObjetivo,
                        idGrupo);

                UsuarioGestionRepository repository =
                    CrearUsuarioRepository();

                datos.IdUsuarioObjetivo =
                    repository.Insertar(
                        usuario,
                        new[] { idGrupo },
                        CrearAuditoria(
                            datos,
                            "Alta",
                            null,
                            datos.Marca +
                            " alta de usuario"));

                AuditoriaListadoDto evento =
                    CrearAuditoriaRepository()
                        .Listar(
                            new AuditoriaFiltro(
                                null,
                                null,
                                datos.NombreActor,
                                "Seguridad",
                                "Alta",
                                datos.Marca))
                        .SingleOrDefault();

                Assert.IsNotNull(
                    evento);

                Assert.AreEqual(
                    datos.IdUsuarioObjetivo,
                    evento.IdEntidad);

                Assert.AreEqual(
                    "Usuario",
                    evento.Entidad);

                Assert.IsNotNull(
                    repository.ObtenerPorId(
                        datos.IdUsuarioObjetivo));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_ConActorInexistente_RevierteUsuarioYGrupos()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                datos.IdPersonaObjetivo =
                    InsertarPersona(
                        datos.EmailObjetivo);

                int idGrupo =
                    ObtenerIdGrupo(
                        "COMERCIAL");

                Usuario usuario =
                    CrearUsuarioNuevo(
                        datos.IdPersonaObjetivo,
                        datos.NombreUsuarioObjetivo,
                        idGrupo);

                AuditoriaRegistro auditoria =
                    CrearAuditoriaInvalida(
                        datos,
                        "Alta",
                        null);

                UsuarioGestionRepository repository =
                    CrearUsuarioRepository();

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Insertar(
                            usuario,
                            new[] { idGrupo },
                            auditoria));

                Assert.IsFalse(
                    repository.PersonaTieneUsuario(
                        datos.IdPersonaObjetivo));

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
        public void Actualizar_ConAuditoriaValida_PersisteCambiosYEvento()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                InsertarActor(
                    datos);

                CrearUsuarioObjetivo(
                    datos,
                    "COMERCIAL");

                UsuarioGestionRepository repository =
                    CrearUsuarioRepository();

                Usuario usuario =
                    repository.ObtenerPorId(
                        datos.IdUsuarioObjetivo);

                int idGrupoNuevo =
                    ObtenerIdGrupo(
                        "ADMINISTRATIVO");

                string nombreNuevo =
                    datos.NombreUsuarioObjetivo +
                    "_mod";

                usuario.ActualizarNombreUsuario(
                    nombreNuevo);

                usuario.ReemplazarGrupos(
                    new[]
                    {
                        CrearGrupoDesdeBase(
                            idGrupoNuevo)
                    });

                repository.Actualizar(
                    usuario,
                    new[] { idGrupoNuevo },
                    CrearAuditoria(
                        datos,
                        "Modificacion",
                        datos.IdUsuarioObjetivo,
                        datos.Marca +
                        " modificación válida"));

                Usuario persistido =
                    repository.ObtenerPorId(
                        datos.IdUsuarioObjetivo);

                Assert.AreEqual(
                    nombreNuevo,
                    persistido.NombreUsuario);

                CollectionAssert.AreEquivalent(
                    new[] { "ADMINISTRATIVO" },
                    persistido.Grupos
                        .Select(
                            grupo => grupo.Codigo)
                        .ToArray());

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
        public void Actualizar_ConActorInexistente_RevierteNombreYGrupos()
        {
            DatosPrueba datos =
                CrearDatosPrueba();

            try
            {
                CrearUsuarioObjetivo(
                    datos,
                    "COMERCIAL");

                UsuarioGestionRepository repository =
                    CrearUsuarioRepository();

                Usuario usuario =
                    repository.ObtenerPorId(
                        datos.IdUsuarioObjetivo);

                string nombreOriginal =
                    usuario.NombreUsuario;

                int idGrupoNuevo =
                    ObtenerIdGrupo(
                        "ADMINISTRATIVO");

                usuario.ActualizarNombreUsuario(
                    datos.NombreUsuarioObjetivo +
                    "_fallo");

                usuario.ReemplazarGrupos(
                    new[]
                    {
                        CrearGrupoDesdeBase(
                            idGrupoNuevo)
                    });

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Actualizar(
                            usuario,
                            new[] { idGrupoNuevo },
                            CrearAuditoriaInvalida(
                                datos,
                                "Modificacion",
                                datos.IdUsuarioObjetivo)));

                Usuario persistido =
                    repository.ObtenerPorId(
                        datos.IdUsuarioObjetivo);

                Assert.AreEqual(
                    nombreOriginal,
                    persistido.NombreUsuario);

                CollectionAssert.AreEquivalent(
                    new[] { "COMERCIAL" },
                    persistido.Grupos
                        .Select(
                            grupo => grupo.Codigo)
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

                CrearUsuarioObjetivo(
                    datos,
                    "COMERCIAL");

                UsuarioGestionRepository repository =
                    CrearUsuarioRepository();

                repository.Desactivar(
                    datos.IdUsuarioObjetivo,
                    CrearAuditoria(
                        datos,
                        "Desactivacion",
                        datos.IdUsuarioObjetivo,
                        datos.Marca +
                        " desactivación"));

                repository.Activar(
                    datos.IdUsuarioObjetivo,
                    CrearAuditoria(
                        datos,
                        "Activacion",
                        datos.IdUsuarioObjetivo,
                        datos.Marca +
                        " activación"));

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
                        actual =>
                            actual.Accion ==
                            "Desactivacion"));

                Assert.IsTrue(
                    eventos.Any(
                        actual =>
                            actual.Accion ==
                            "Activacion"));

                Assert.IsTrue(
                    repository
                        .ObtenerPorId(
                            datos.IdUsuarioObjetivo)
                        .Activo);
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
                CrearUsuarioObjetivo(
                    datos,
                    "COMERCIAL");

                UsuarioGestionRepository repository =
                    CrearUsuarioRepository();

                Assert.ThrowsException
                    <PersistenciaException>(
                        () => repository.Desactivar(
                            datos.IdUsuarioObjetivo,
                            CrearAuditoriaInvalida(
                                datos,
                                "Desactivacion",
                                datos.IdUsuarioObjetivo)));

                Usuario persistido =
                    repository.ObtenerPorId(
                        datos.IdUsuarioObjetivo);

                Assert.IsTrue(
                    persistido.Activo);

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

        private static UsuarioGestionRepository
            CrearUsuarioRepository()
        {
            return new UsuarioGestionRepository(
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

            string marca =
                "USUARIO_AUDIT_TEST_" +
                sufijo;

            return new DatosPrueba
            {
                Marca =
                    marca,

                NombreActor =
                    "actor_" +
                    sufijo,

                EmailActor =
                    "actor_" +
                    sufijo +
                    "@sigevip.test",

                NombreUsuarioObjetivo =
                    "objetivo_" +
                    sufijo,

                EmailObjetivo =
                    "objetivo_" +
                    sufijo +
                    "@sigevip.test"
            };
        }

        private static void InsertarActor(
            DatosPrueba datos)
        {
            datos.IdPersonaActor =
                InsertarPersona(
                    datos.EmailActor);

            PasswordHashResult hash =
                CrearHash();

            datos.IdUsuarioActor =
                InsertarUsuarioDirecto(
                    datos.IdPersonaActor,
                    datos.NombreActor,
                    hash,
                    true);
        }

        private static void CrearUsuarioObjetivo(
            DatosPrueba datos,
            string codigoGrupo)
        {
            datos.IdPersonaObjetivo =
                InsertarPersona(
                    datos.EmailObjetivo);

            PasswordHashResult hash =
                CrearHash();

            int idGrupo =
                ObtenerIdGrupo(
                    codigoGrupo);

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
                        datos.IdUsuarioObjetivo =
                            InsertarUsuarioDirecto(
                                connection,
                                transaction,
                                datos.IdPersonaObjetivo,
                                datos.NombreUsuarioObjetivo,
                                hash,
                                true);

                        InsertarUsuarioGrupo(
                            connection,
                            transaction,
                            datos.IdUsuarioObjetivo,
                            idGrupo);

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

        private static Usuario CrearUsuarioNuevo(
            int idPersona,
            string nombreUsuario,
            int idGrupo)
        {
            PasswordHashResult hash =
                CrearHash();

            Usuario usuario =
                new Usuario(
                    0,
                    idPersona,
                    nombreUsuario,
                    hash.Hash,
                    hash.Salt,
                    hash.Iteraciones);

            usuario.ReemplazarGrupos(
                new[]
                {
                    CrearGrupoDesdeBase(
                        idGrupo)
                });

            return usuario;
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
                "Usuario",
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
                "actor.inexistente",
                "Seguridad",
                accion,
                "Usuario",
                idEntidad,
                datos.Marca +
                " auditoría inválida");
        }

        private static PasswordHashResult CrearHash()
        {
            return new Pbkdf2PasswordHasher()
                .CrearHash(
                    "ClaveIntegracion123");
        }

        private static int InsertarPersona(
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
    N'Usuario',
    N'Auditoria',
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
                    "@Email",
                    SqlDbType.NVarChar,
                    254).Value =
                        email;

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int InsertarUsuarioDirecto(
            int idPersona,
            string nombreUsuario,
            PasswordHashResult hash,
            bool activo)
        {
            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            {
                connection.Open();

                return InsertarUsuarioDirecto(
                    connection,
                    null,
                    idPersona,
                    nombreUsuario,
                    hash,
                    activo);
            }
        }

        private static int InsertarUsuarioDirecto(
            SqlConnection connection,
            SqlTransaction transaction,
            int idPersona,
            string nombreUsuario,
            PasswordHashResult hash,
            bool activo)
        {
            const string sql = @"
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
    @IteracionesPassword,
    @Activo
);

SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (
                SqlCommand command =
                    transaction == null
                        ? new SqlCommand(
                            sql,
                            connection)
                        : new SqlCommand(
                            sql,
                            connection,
                            transaction))
            {
                command.Parameters.Add(
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        idPersona;

                command.Parameters.Add(
                    "@NombreUsuario",
                    SqlDbType.NVarChar,
                    100).Value =
                        nombreUsuario;

                command.Parameters.Add(
                    "@PasswordHash",
                    SqlDbType.VarBinary,
                    hash.Hash.Length).Value =
                        hash.Hash;

                command.Parameters.Add(
                    "@PasswordSalt",
                    SqlDbType.VarBinary,
                    hash.Salt.Length).Value =
                        hash.Salt;

                command.Parameters.Add(
                    "@IteracionesPassword",
                    SqlDbType.Int).Value =
                        hash.Iteraciones;

                command.Parameters.Add(
                    "@Activo",
                    SqlDbType.Bit).Value =
                        activo;

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static void InsertarUsuarioGrupo(
            SqlConnection connection,
            SqlTransaction transaction,
            int idUsuario,
            int idGrupo)
        {
            const string sql = @"
INSERT INTO dbo.UsuarioGrupo
(
    IdUsuario,
    IdGrupo
)
VALUES
(
    @IdUsuario,
    @IdGrupo
);";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdUsuario",
                    SqlDbType.Int).Value =
                        idUsuario;

                command.Parameters.Add(
                    "@IdGrupo",
                    SqlDbType.Int).Value =
                        idGrupo;

                command.ExecuteNonQuery();
            }
        }

        private static int ObtenerIdGrupo(
            string codigo)
        {
            const string sql = @"
SELECT IdGrupo
FROM dbo.Grupo
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
                        "No existe el grupo activo " +
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
                    if (!reader.Read())
                    {
                        Assert.Fail(
                            "No existe el grupo requerido.");
                    }

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

DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario IN
(
    @IdUsuarioObjetivo,
    @IdUsuarioActor
);

DELETE FROM dbo.Usuario
WHERE IdUsuario IN
(
    @IdUsuarioObjetivo,
    @IdUsuarioActor
);

DELETE FROM dbo.Persona
WHERE IdPersona IN
(
    @IdPersonaObjetivo,
    @IdPersonaActor
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
                    "@IdUsuarioActor",
                    SqlDbType.Int).Value =
                        datos.IdUsuarioActor;

                command.Parameters.Add(
                    "@IdUsuarioObjetivo",
                    SqlDbType.Int).Value =
                        datos.IdUsuarioObjetivo;

                command.Parameters.Add(
                    "@IdPersonaActor",
                    SqlDbType.Int).Value =
                        datos.IdPersonaActor;

                command.Parameters.Add(
                    "@IdPersonaObjetivo",
                    SqlDbType.Int).Value =
                        datos.IdPersonaObjetivo;

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

            public int IdPersonaObjetivo
            {
                get;
                set;
            }

            public int IdUsuarioObjetivo
            {
                get;
                set;
            }

            public string NombreUsuarioObjetivo
            {
                get;
                set;
            }

            public string EmailObjetivo
            {
                get;
                set;
            }
        }
    }
}