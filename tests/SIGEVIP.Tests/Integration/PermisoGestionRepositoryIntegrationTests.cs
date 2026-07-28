using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Permisos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Permisos;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class PermisoGestionRepositoryIntegrationTests
    {
        [TestMethod]
        public void Listar_PermisoPersistido_RecuperaDatos()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    true,
                    true);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                PermisoListadoDto permiso =
                    repository.Listar(
                            new PermisoFiltro(
                                datos.Codigo,
                                null))
                        .Single(
                            actual =>
                                actual.IdPermiso ==
                                datos.IdPermiso);

                Assert.AreEqual(
                    datos.Codigo,
                    permiso.Codigo);

                Assert.AreEqual(
                    datos.Nombre,
                    permiso.Nombre);

                Assert.AreEqual(
                    datos.Descripcion,
                    permiso.Descripcion);

                Assert.AreEqual(
                    1,
                    permiso.CantidadGrupos);

                Assert.IsTrue(
                    permiso.Activo);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_FiltroPorNombre_EncuentraPermiso()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    true,
                    false);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                IReadOnlyCollection<PermisoListadoDto>
                    resultados =
                        repository.Listar(
                            new PermisoFiltro(
                                datos.Nombre,
                                null));

                Assert.IsTrue(
                    resultados.Any(
                        actual =>
                            actual.IdPermiso ==
                            datos.IdPermiso));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_FiltroPorDescripcion_EncuentraPermiso()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    true,
                    false);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                IReadOnlyCollection<PermisoListadoDto>
                    resultados =
                        repository.Listar(
                            new PermisoFiltro(
                                datos.Sufijo,
                                null));

                Assert.IsTrue(
                    resultados.Any(
                        actual =>
                            actual.IdPermiso ==
                            datos.IdPermiso));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_FiltroActivo_RecuperaPermisoActivo()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    true,
                    false);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                PermisoListadoDto permiso =
                    repository.Listar(
                            new PermisoFiltro(
                                datos.Codigo,
                                true))
                        .Single(
                            actual =>
                                actual.IdPermiso ==
                                datos.IdPermiso);

                Assert.IsTrue(
                    permiso.Activo);

                Assert.AreEqual(
                    "Activo",
                    permiso.Estado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_FiltroInactivo_RecuperaPermisoInactivo()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    false,
                    false);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                PermisoListadoDto permiso =
                    repository.Listar(
                            new PermisoFiltro(
                                datos.Codigo,
                                false))
                        .Single(
                            actual =>
                                actual.IdPermiso ==
                                datos.IdPermiso);

                Assert.IsFalse(
                    permiso.Activo);

                Assert.AreEqual(
                    "Inactivo",
                    permiso.Estado);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerDetallePorId_PermisoPersistido_RecuperaCantidadGrupos()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    true,
                    true);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                PermisoDetalleDto detalle =
                    repository.ObtenerDetallePorId(
                        datos.IdPermiso);

                Assert.IsNotNull(
                    detalle);

                Assert.AreEqual(
                    datos.IdPermiso,
                    detalle.IdPermiso);

                Assert.AreEqual(
                    datos.Codigo,
                    detalle.Codigo);

                Assert.AreEqual(
                    1,
                    detalle.CantidadGrupos);

                Assert.IsTrue(
                    detalle.Activo);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerDetallePorId_PermisoInexistente_DevuelveNull()
        {
            PermisoGestionRepository repository =
                CrearRepository();

            PermisoDetalleDto detalle =
                repository.ObtenerDetallePorId(
                    int.MaxValue);

            Assert.IsNull(
                detalle);
        }

        [TestMethod]
        public void ObtenerPorId_PermisoActivo_ReconstruyeEntidad()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    true,
                    false);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                Permiso permiso =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                Assert.IsNotNull(
                    permiso);

                Assert.AreEqual(
                    datos.Codigo,
                    permiso.Codigo);

                Assert.AreEqual(
                    datos.Nombre,
                    permiso.Nombre);

                Assert.AreEqual(
                    datos.Descripcion,
                    permiso.Descripcion);

                Assert.IsTrue(
                    permiso.Activo);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerPorId_PermisoInactivo_ReconstruyeEstado()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    false,
                    false);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                Permiso permiso =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                Assert.IsNotNull(
                    permiso);

                Assert.IsFalse(
                    permiso.Activo);

                Assert.AreEqual(
                    0,
                    permiso
                        .ObtenerPermisosEfectivos()
                        .Count);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ExisteCodigo_PermisoPersistido_DevuelveTrue()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    true,
                    false);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                Assert.IsTrue(
                    repository.ExisteCodigo(
                        datos.Codigo,
                        null));

                Assert.IsFalse(
                    repository.ExisteCodigo(
                        datos.Codigo,
                        datos.IdPermiso));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_DatosValidos_PersistePermiso()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            Permiso permiso =
                new Permiso(
                    0,
                    "ALTA_PERMISO_" +
                    sufijo,
                    "Alta permiso " +
                    sufijo,
                    "Permiso creado por prueba " +
                    sufijo);

            int idPermiso = 0;

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                idPermiso =
                    repository.Insertar(
                        permiso);

                Permiso persistido =
                    repository.ObtenerPorId(
                        idPermiso);

                Assert.IsNotNull(
                    persistido);

                Assert.AreEqual(
                    permiso.Codigo,
                    persistido.Codigo);

                Assert.AreEqual(
                    permiso.Nombre,
                    persistido.Nombre);

                Assert.AreEqual(
                    permiso.Descripcion,
                    persistido.Descripcion);

                Assert.IsTrue(
                    persistido.Activo);
            }
            finally
            {
                EliminarPermisoPorId(
                    idPermiso);
            }
        }

        [TestMethod]
        public void Insertar_DescripcionVacia_PersisteYRecuperaTextoVacio()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            Permiso permiso =
                new Permiso(
                    0,
                    "PERMISO_SIN_DESCRIPCION_" +
                    sufijo,
                    "Permiso sin descripción " +
                    sufijo,
                    string.Empty);

            int idPermiso = 0;

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                idPermiso =
                    repository.Insertar(
                        permiso);

                Permiso persistido =
                    repository.ObtenerPorId(
                        idPermiso);

                Assert.IsNotNull(
                    persistido);

                Assert.AreEqual(
                    string.Empty,
                    persistido.Descripcion);

                Assert.IsTrue(
                    DescripcionEsNulaEnBase(
                        idPermiso));
            }
            finally
            {
                EliminarPermisoPorId(
                    idPermiso);
            }
        }

        [TestMethod]
        public void Insertar_CodigoDuplicado_RechazaOperacion()
        {
            DatosPermisoPrueba existente =
                CrearPermisoPrueba(
                    true,
                    false);

            try
            {
                Permiso duplicado =
                    new Permiso(
                        0,
                        existente.Codigo,
                        "Otro permiso " +
                        existente.Sufijo,
                        "Descripción duplicada");

                PermisoGestionRepository repository =
                    CrearRepository();

                Assert.ThrowsException<ReglaNegocioException>(
                    () => repository.Insertar(
                        duplicado));

                Assert.AreEqual(
                    1,
                    ContarPermisosPorCodigo(
                        existente.Codigo));
            }
            finally
            {
                EliminarDatosPrueba(
                    existente);
            }
        }

        [TestMethod]
        public void Actualizar_DatosValidos_ModificaNombreYDescripcion()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    true,
                    false);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                Permiso permiso =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                string nombreNuevo =
                    "Permiso modificado " +
                    datos.Sufijo;

                string descripcionNueva =
                    "Descripción modificada " +
                    datos.Sufijo;

                permiso.ActualizarDatos(
                    nombreNuevo,
                    descripcionNueva);

                repository.Actualizar(
                    permiso);

                Permiso persistido =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                Assert.AreEqual(
                    nombreNuevo,
                    persistido.Nombre);

                Assert.AreEqual(
                    descripcionNueva,
                    persistido.Descripcion);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_DatosValidos_PreservaCodigoYEstado()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    false,
                    false);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                Permiso permiso =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                permiso.ActualizarDatos(
                    "Nombre actualizado " +
                    datos.Sufijo,
                    "Descripción actualizada " +
                    datos.Sufijo);

                repository.Actualizar(
                    permiso);

                Permiso persistido =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                Assert.AreEqual(
                    datos.Codigo,
                    persistido.Codigo);

                Assert.IsFalse(
                    persistido.Activo);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_PermisoAsociado_PreservaGrupoPermiso()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    true,
                    true);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                Permiso permiso =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                permiso.ActualizarDatos(
                    "Permiso asociado modificado " +
                    datos.Sufijo,
                    "Descripción modificada " +
                    datos.Sufijo);

                repository.Actualizar(
                    permiso);

                Assert.IsTrue(
                    ExisteGrupoPermiso(
                        datos.IdGrupo,
                        datos.IdPermiso));

                PermisoDetalleDto detalle =
                    repository.ObtenerDetallePorId(
                        datos.IdPermiso);

                Assert.AreEqual(
                    1,
                    detalle.CantidadGrupos);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void DesactivarYActivar_PermisoPersistido_ConservaAsociacion()
        {
            DatosPermisoPrueba datos =
                CrearPermisoPrueba(
                    true,
                    true);

            try
            {
                PermisoGestionRepository repository =
                    CrearRepository();

                repository.Desactivar(
                    datos.IdPermiso);

                Permiso inactivo =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                Assert.IsFalse(
                    inactivo.Activo);

                Assert.IsTrue(
                    ExisteGrupoPermiso(
                        datos.IdGrupo,
                        datos.IdPermiso));

                repository.Activar(
                    datos.IdPermiso);

                Permiso activo =
                    repository.ObtenerPorId(
                        datos.IdPermiso);

                Assert.IsTrue(
                    activo.Activo);

                Assert.IsTrue(
                    ExisteGrupoPermiso(
                        datos.IdGrupo,
                        datos.IdPermiso));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_PermisoInexistente_LanzaPersistenciaException()
        {
            Permiso permiso =
                new Permiso(
                    int.MaxValue,
                    "PERMISO_INEXISTENTE",
                    "Permiso inexistente",
                    "Prueba de registro inexistente");

            PermisoGestionRepository repository =
                CrearRepository();

            Assert.ThrowsException<PersistenciaException>(
                () => repository.Actualizar(
                    permiso));
        }

        [TestMethod]
        public void Desactivar_PermisoInexistente_LanzaPersistenciaException()
        {
            PermisoGestionRepository repository =
                CrearRepository();

            Assert.ThrowsException<PersistenciaException>(
                () => repository.Desactivar(
                    int.MaxValue));
        }

        private static PermisoGestionRepository
            CrearRepository()
        {
            return new PermisoGestionRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosPermisoPrueba
            CrearPermisoPrueba(
                bool activo,
                bool asociarGrupo)
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N")
                    .ToUpperInvariant();

            DatosPermisoPrueba datos =
                new DatosPermisoPrueba(
                    sufijo,
                    "PERMISO_INTEGRACION_" +
                    sufijo,
                    "Permiso integración " +
                    sufijo,
                    "Descripción integración " +
                    sufijo);

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
                        datos.IdPermiso =
                            InsertarPermiso(
                                connection,
                                transaction,
                                datos.Codigo,
                                datos.Nombre,
                                datos.Descripcion,
                                activo);

                        if (asociarGrupo)
                        {
                            datos.IdGrupo =
                                InsertarGrupo(
                                    connection,
                                    transaction,
                                    "GRUPO_PERMISO_" +
                                    sufijo,
                                    "Grupo permiso " +
                                    sufijo);

                            InsertarGrupoPermiso(
                                connection,
                                transaction,
                                datos.IdGrupo,
                                datos.IdPermiso);
                        }

                        transaction.Commit();

                        return datos;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static int InsertarPermiso(
            SqlConnection connection,
            SqlTransaction transaction,
            string codigo,
            string nombre,
            string descripcion,
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
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
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
                    "@Activo",
                    SqlDbType.Bit).Value =
                        activo;

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int InsertarGrupo(
            SqlConnection connection,
            SqlTransaction transaction,
            string codigo,
            string nombre)
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
    @Codigo,
    @Nombre,
    @Descripcion,
    1
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
                        "Grupo temporal para pruebas de Permisos.";

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static void InsertarGrupoPermiso(
            SqlConnection connection,
            SqlTransaction transaction,
            int idGrupo,
            int idPermiso)
        {
            const string sql = @"
INSERT INTO dbo.GrupoPermiso
(
    IdGrupo,
    IdPermiso
)
VALUES
(
    @IdGrupo,
    @IdPermiso
);";

            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdGrupo",
                    SqlDbType.Int).Value =
                        idGrupo;

                command.Parameters.Add(
                    "@IdPermiso",
                    SqlDbType.Int).Value =
                        idPermiso;

                command.ExecuteNonQuery();
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

        private static bool DescripcionEsNulaEnBase(
            int idPermiso)
        {
            const string sql = @"
SELECT
    CASE
        WHEN Descripcion IS NULL
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END
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

                return Convert.ToBoolean(
                    command.ExecuteScalar());
            }
        }

        private static int ContarPermisosPorCodigo(
            string codigo)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Permiso
WHERE Codigo = @Codigo;";

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

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static void EliminarDatosPrueba(
            DatosPermisoPrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            const string sql = @"
DELETE FROM dbo.GrupoPermiso
WHERE
    IdPermiso = @IdPermiso
    OR IdGrupo = @IdGrupo;

DELETE FROM dbo.UsuarioGrupo
WHERE IdGrupo = @IdGrupo;

DELETE FROM dbo.GrupoGrupo
WHERE
    IdGrupoPadre = @IdGrupo
    OR IdGrupoHijo = @IdGrupo;

DELETE FROM dbo.Grupo
WHERE IdGrupo = @IdGrupo;

DELETE FROM dbo.Permiso
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
                        datos.IdPermiso;

                command.Parameters.Add(
                    "@IdGrupo",
                    SqlDbType.Int).Value =
                        datos.IdGrupo;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void EliminarPermisoPorId(
            int idPermiso)
        {
            if (idPermiso <= 0)
            {
                return;
            }

            const string sql = @"
DELETE FROM dbo.GrupoPermiso
WHERE IdPermiso = @IdPermiso;

DELETE FROM dbo.Permiso
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

        private sealed class DatosPermisoPrueba
        {
            public DatosPermisoPrueba(
                string sufijo,
                string codigo,
                string nombre,
                string descripcion)
            {
                Sufijo =
                    sufijo;

                Codigo =
                    codigo;

                Nombre =
                    nombre;

                Descripcion =
                    descripcion;
            }

            public int IdPermiso
            {
                get;
                set;
            }

            public int IdGrupo
            {
                get;
                set;
            }

            public string Sufijo
            {
                get;
                private set;
            }

            public string Codigo
            {
                get;
                private set;
            }

            public string Nombre
            {
                get;
                private set;
            }

            public string Descripcion
            {
                get;
                private set;
            }
        }
    }
}