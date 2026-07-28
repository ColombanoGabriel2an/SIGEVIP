using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Usuarios;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Security;
using SIGEVIP.Infrastructure.Usuarios;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class UsuarioGestionRepositoryIntegrationTests
    {
        [TestMethod]
        public void Listar_UsuarioPersistido_RecuperaDatosYGrupos()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    true,
                    "COMERCIAL");

            try
            {
                UsuarioGestionRepository repository =
                    CrearRepository();

                IReadOnlyCollection<UsuarioListadoDto> resultados =
                    repository.Listar(
                        new UsuarioFiltro(
                            datos.NombreUsuario,
                            null,
                            null));

                UsuarioListadoDto usuario =
                    resultados.Single(
                        actual =>
                            actual.IdUsuario ==
                            datos.IdUsuario);

                Assert.AreEqual(
                    datos.NombreUsuario,
                    usuario.NombreUsuario);

                Assert.AreEqual(
                    datos.Email,
                    usuario.Email);

                Assert.IsTrue(
                    usuario.Activo);

                StringAssert.Contains(
                    usuario.GruposResumen,
                    "Comercial");
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_FiltroEstado_DevuelveUsuarioInactivo()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    false,
                    "COMERCIAL");

            try
            {
                UsuarioGestionRepository repository =
                    CrearRepository();

                IReadOnlyCollection<UsuarioListadoDto> resultados =
                    repository.Listar(
                        new UsuarioFiltro(
                            datos.NombreUsuario,
                            false,
                            null));

                UsuarioListadoDto usuario =
                    resultados.Single(
                        actual =>
                            actual.IdUsuario ==
                            datos.IdUsuario);

                Assert.IsFalse(
                    usuario.Activo);

                Assert.AreEqual(
                    "Inactivo",
                    usuario.Estado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_FiltroGrupo_DevuelveSoloUsuarioAsignado()
        {
            DatosPrueba datosComercial =
                CrearDatosPrueba(
                    true,
                    "COMERCIAL");

            DatosPrueba datosAdministrativo =
                CrearDatosPrueba(
                    true,
                    "ADMINISTRATIVO");

            try
            {
                UsuarioGestionRepository repository =
                    CrearRepository();

                int idGrupoComercial =
                    ObtenerIdGrupo(
                        "COMERCIAL");

                IReadOnlyCollection<UsuarioListadoDto> resultados =
                    repository.Listar(
                        new UsuarioFiltro(
                            "integracion_usuario_",
                            null,
                            idGrupoComercial));

                Assert.IsTrue(
                    resultados.Any(
                        actual =>
                            actual.IdUsuario ==
                            datosComercial.IdUsuario));

                Assert.IsFalse(
                    resultados.Any(
                        actual =>
                            actual.IdUsuario ==
                            datosAdministrativo.IdUsuario));
            }
            finally
            {
                EliminarDatosPrueba(
                    datosAdministrativo);

                EliminarDatosPrueba(
                    datosComercial);
            }
        }

        [TestMethod]
        public void ObtenerDetallePorId_UsuarioPersistido_RecuperaGruposDirectos()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    true,
                    "COMERCIAL",
                    "GERENTE");

            try
            {
                UsuarioGestionRepository repository =
                    CrearRepository();

                UsuarioDetalleDto detalle =
                    repository.ObtenerDetallePorId(
                        datos.IdUsuario);

                Assert.IsNotNull(
                    detalle);

                Assert.AreEqual(
                    datos.IdUsuario,
                    detalle.IdUsuario);

                Assert.AreEqual(
                    datos.IdPersona,
                    detalle.IdPersona);

                Assert.AreEqual(
                    datos.NombreUsuario,
                    detalle.NombreUsuario);

                CollectionAssert.AreEquivalent(
                    datos.IdsGrupos.ToArray(),
                    detalle.IdsGrupos.ToArray());
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ListarPersonasDisponibles_ExcluyePersonaConUsuario()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    true,
                    "COMERCIAL");

            DatosPersonaPrueba personaDisponible =
                CrearPersonaPrueba(
                    true);

            try
            {
                UsuarioGestionRepository repository =
                    CrearRepository();

                IReadOnlyCollection<PersonaSeleccionUsuarioDto>
                    personas =
                        repository.ListarPersonasDisponibles();

                Assert.IsFalse(
                    personas.Any(
                        actual =>
                            actual.IdPersona ==
                            datos.IdPersona));

                Assert.IsTrue(
                    personas.Any(
                        actual =>
                            actual.IdPersona ==
                            personaDisponible.IdPersona));
            }
            finally
            {
                EliminarPersonaPrueba(
                    personaDisponible);

                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_DatosValidos_PersisteUsuarioYGrupos()
        {
            DatosPersonaPrueba persona =
                CrearPersonaPrueba(
                    true);

            int idGrupoComercial =
                ObtenerIdGrupo(
                    "COMERCIAL");

            int idGrupoAdministrativo =
                ObtenerIdGrupo(
                    "ADMINISTRATIVO");

            int idUsuario = 0;

            try
            {
                string nombreUsuario =
                    "integracion_alta_" +
                    Guid.NewGuid()
                        .ToString("N");

                PasswordHashResult hash =
                    new Pbkdf2PasswordHasher()
                        .CrearHash(
                            "ClaveIntegracion123");

                Usuario usuario =
                    new Usuario(
                        0,
                        persona.IdPersona,
                        nombreUsuario,
                        hash.Hash,
                        hash.Salt,
                        hash.Iteraciones);

                usuario.ReemplazarGrupos(
                    new[]
                    {
                        CrearGrupoDesdeBase(
                            idGrupoComercial),
                        CrearGrupoDesdeBase(
                            idGrupoAdministrativo)
                    });

                UsuarioGestionRepository repository =
                    CrearRepository();

                idUsuario =
                    repository.Insertar(
                        usuario,
                        new[]
                        {
                            idGrupoComercial,
                            idGrupoAdministrativo
                        });

                Usuario persistido =
                    repository.ObtenerPorId(
                        idUsuario);

                Assert.IsNotNull(
                    persistido);

                Assert.AreEqual(
                    nombreUsuario,
                    persistido.NombreUsuario);

                Assert.AreEqual(
                    persona.IdPersona,
                    persistido.IdPersona);

                Assert.IsTrue(
                    persistido.Activo);

                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        "COMERCIAL",
                        "ADMINISTRATIVO"
                    },
                    persistido.Grupos
                        .Select(
                            grupo => grupo.Codigo)
                        .ToArray());
            }
            finally
            {
                EliminarUsuarioYPersona(
                    idUsuario,
                    persona.IdPersona);
            }
        }

        [TestMethod]
        public void Insertar_NombreDuplicado_RechazaYNoCreaSegundoUsuario()
        {
            DatosPrueba existente =
                CrearDatosPrueba(
                    true,
                    "COMERCIAL");

            DatosPersonaPrueba personaNueva =
                CrearPersonaPrueba(
                    true);

            try
            {
                int idGrupo =
                    ObtenerIdGrupo(
                        "COMERCIAL");

                PasswordHashResult hash =
                    new Pbkdf2PasswordHasher()
                        .CrearHash(
                            "ClaveIntegracion123");

                Usuario usuario =
                    new Usuario(
                        0,
                        personaNueva.IdPersona,
                        existente.NombreUsuario,
                        hash.Hash,
                        hash.Salt,
                        hash.Iteraciones);

                usuario.AgregarGrupo(
                    CrearGrupoDesdeBase(
                        idGrupo));

                UsuarioGestionRepository repository =
                    CrearRepository();

                Assert.ThrowsException<ReglaNegocioException>(
                    () => repository.Insertar(
                        usuario,
                        new[] { idGrupo }));

                Assert.IsFalse(
                    repository.PersonaTieneUsuario(
                        personaNueva.IdPersona));
            }
            finally
            {
                EliminarPersonaPrueba(
                    personaNueva);

                EliminarDatosPrueba(
                    existente);
            }
        }

        [TestMethod]
        public void Actualizar_DatosValidos_ModificaNombreYReemplazaGrupos()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    true,
                    "COMERCIAL");

            try
            {
                UsuarioGestionRepository repository =
                    CrearRepository();

                Usuario usuario =
                    repository.ObtenerPorId(
                        datos.IdUsuario);

                int idGrupoAdministrativo =
                    ObtenerIdGrupo(
                        "ADMINISTRATIVO");

                int idGrupoGerente =
                    ObtenerIdGrupo(
                        "GERENTE");

                string nombreNuevo =
                    "integracion_modificado_" +
                    Guid.NewGuid()
                        .ToString("N");

                usuario.ActualizarNombreUsuario(
                    nombreNuevo);

                usuario.ReemplazarGrupos(
                    new[]
                    {
                        CrearGrupoDesdeBase(
                            idGrupoAdministrativo),
                        CrearGrupoDesdeBase(
                            idGrupoGerente)
                    });

                repository.Actualizar(
                    usuario,
                    new[]
                    {
                        idGrupoAdministrativo,
                        idGrupoGerente
                    });

                Usuario persistido =
                    repository.ObtenerPorId(
                        datos.IdUsuario);

                Assert.AreEqual(
                    nombreNuevo,
                    persistido.NombreUsuario);

                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        "ADMINISTRATIVO",
                        "GERENTE"
                    },
                    persistido.Grupos
                        .Select(
                            grupo => grupo.Codigo)
                        .ToArray());
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void DesactivarYActivar_UsuarioPersistido_ConservaCambios()
        {
            DatosPrueba datos =
                CrearDatosPrueba(
                    true,
                    "COMERCIAL");

            try
            {
                UsuarioGestionRepository repository =
                    CrearRepository();

                repository.Desactivar(
                    datos.IdUsuario);

                Usuario inactivo =
                    repository.ObtenerPorId(
                        datos.IdUsuario);

                Assert.IsFalse(
                    inactivo.Activo);

                repository.Activar(
                    datos.IdUsuario);

                Usuario activo =
                    repository.ObtenerPorId(
                        datos.IdUsuario);

                Assert.IsTrue(
                    activo.Activo);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ExisteOtroAdministradorActivo_ConSegundoAdministrador_DevuelveTrue()
        {
            DatosPrueba primerAdministrador =
                CrearDatosPrueba(
                    true,
                    "ADMINISTRADOR_GENERAL");

            DatosPrueba segundoAdministrador =
                CrearDatosPrueba(
                    true,
                    "ADMINISTRADOR_GENERAL");

            try
            {
                UsuarioGestionRepository repository =
                    CrearRepository();

                bool existeOtro =
                    repository.ExisteOtroAdministradorActivo(
                        primerAdministrador.IdUsuario);

                Assert.IsTrue(
                    existeOtro);
            }
            finally
            {
                EliminarDatosPrueba(
                    segundoAdministrador);

                EliminarDatosPrueba(
                    primerAdministrador);
            }
        }

        private static UsuarioGestionRepository CrearRepository()
        {
            return new UsuarioGestionRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosPrueba CrearDatosPrueba(
            bool activo,
            params string[] codigosGrupos)
        {
            if (codigosGrupos == null ||
                codigosGrupos.Length == 0)
            {
                throw new ArgumentException(
                    "Debe indicar al menos un grupo.",
                    nameof(codigosGrupos));
            }

            DatosPersonaPrueba persona =
                CrearPersonaPrueba(
                    true);

            string nombreUsuario =
                "integracion_usuario_" +
                Guid.NewGuid()
                    .ToString("N");

            PasswordHashResult hash =
                new Pbkdf2PasswordHasher()
                    .CrearHash(
                        "ClaveIntegracion123");

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
                        int idUsuario =
                            InsertarUsuario(
                                connection,
                                transaction,
                                persona.IdPersona,
                                nombreUsuario,
                                hash,
                                activo);

                        List<int> idsGrupos =
                            new List<int>();

                        foreach (
                            string codigoGrupo
                            in codigosGrupos)
                        {
                            int idGrupo =
                                ObtenerIdGrupo(
                                    connection,
                                    transaction,
                                    codigoGrupo);

                            InsertarUsuarioGrupo(
                                connection,
                                transaction,
                                idUsuario,
                                idGrupo);

                            idsGrupos.Add(
                                idGrupo);
                        }

                        transaction.Commit();

                        return new DatosPrueba(
                            persona.IdPersona,
                            idUsuario,
                            nombreUsuario,
                            persona.Email,
                            idsGrupos);
                    }
                    catch
                    {
                        transaction.Rollback();

                        EliminarPersonaPrueba(
                            persona);

                        throw;
                    }
                }
            }
        }

        private static DatosPersonaPrueba CrearPersonaPrueba(
            bool activo)
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            string email =
                "integracion_persona_" +
                sufijo +
                "@sigevip.test";

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
                    "@Nombre",
                    SqlDbType.NVarChar,
                    100).Value =
                        "Persona";

                command.Parameters.Add(
                    "@Apellido",
                    SqlDbType.NVarChar,
                    100).Value =
                        "Integración";

                command.Parameters.Add(
                    "@Email",
                    SqlDbType.NVarChar,
                    254).Value =
                        email;

                command.Parameters.Add(
                    "@Activo",
                    SqlDbType.Bit).Value =
                        activo;

                connection.Open();

                int idPersona =
                    Convert.ToInt32(
                        command.ExecuteScalar());

                return new DatosPersonaPrueba(
                    idPersona,
                    email);
            }
        }

        private static int InsertarUsuario(
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
                    new SqlCommand(
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
            string codigoGrupo)
        {
            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            {
                connection.Open();

                return ObtenerIdGrupo(
                    connection,
                    null,
                    codigoGrupo);
            }
        }

        private static int ObtenerIdGrupo(
            SqlConnection connection,
            SqlTransaction transaction,
            string codigoGrupo)
        {
            const string sql = @"
SELECT
    g.IdGrupo
FROM dbo.Grupo AS g
WHERE
    g.Codigo = @Codigo
    AND g.Activo = 1;";

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
                    "@Codigo",
                    SqlDbType.NVarChar,
                    100).Value =
                        codigoGrupo;

                object resultado =
                    command.ExecuteScalar();

                if (resultado == null ||
                    resultado == DBNull.Value)
                {
                    Assert.Fail(
                        "No existe el grupo activo " +
                        codigoGrupo +
                        ". Ejecute el seed de seguridad.");
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
    g.IdGrupo,
    g.Codigo,
    g.Nombre,
    g.Descripcion,
    g.Activo
FROM dbo.Grupo AS g
WHERE g.IdGrupo = @IdGrupo;";

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
                            "No existe el grupo requerido para la prueba.");
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

                    bool activo =
                        reader.GetBoolean(
                            reader.GetOrdinal(
                                "Activo"));

                    if (!activo)
                    {
                        grupo.Desactivar();
                    }

                    return grupo;
                }
            }
        }

        private static void EliminarDatosPrueba(
            DatosPrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            EliminarUsuarioYPersona(
                datos.IdUsuario,
                datos.IdPersona);
        }

        private static void EliminarUsuarioYPersona(
            int idUsuario,
            int idPersona)
        {
            const string sql = @"
DELETE FROM dbo.UsuarioGrupo
WHERE IdUsuario = @IdUsuario;

DELETE FROM dbo.Usuario
WHERE IdUsuario = @IdUsuario;

DELETE FROM dbo.Persona
WHERE IdPersona = @IdPersona;";

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
                    "@IdUsuario",
                    SqlDbType.Int).Value =
                        idUsuario;

                command.Parameters.Add(
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        idPersona;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void EliminarPersonaPrueba(
            DatosPersonaPrueba persona)
        {
            if (persona == null)
            {
                return;
            }

            const string sql = @"
DELETE FROM dbo.Persona
WHERE IdPersona = @IdPersona
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.Usuario AS u
    WHERE u.IdPersona = @IdPersona
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
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        persona.IdPersona;

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
                    "No existe la cadena de conexión SIGEVIP en App.config.");
            }

            return settings.ConnectionString;
        }

        private sealed class DatosPrueba
        {
            public DatosPrueba(
                int idPersona,
                int idUsuario,
                string nombreUsuario,
                string email,
                IEnumerable<int> idsGrupos)
            {
                IdPersona = idPersona;
                IdUsuario = idUsuario;
                NombreUsuario = nombreUsuario;
                Email = email;

                IdsGrupos =
                    idsGrupos.ToList()
                        .AsReadOnly();
            }

            public int IdPersona
            {
                get;
                private set;
            }

            public int IdUsuario
            {
                get;
                private set;
            }

            public string NombreUsuario
            {
                get;
                private set;
            }

            public string Email
            {
                get;
                private set;
            }

            public IReadOnlyCollection<int> IdsGrupos
            {
                get;
                private set;
            }
        }

        private sealed class DatosPersonaPrueba
        {
            public DatosPersonaPrueba(
                int idPersona,
                string email)
            {
                IdPersona = idPersona;
                Email = email;
            }

            public int IdPersona
            {
                get;
                private set;
            }

            public string Email
            {
                get;
                private set;
            }
        }
    }
}
