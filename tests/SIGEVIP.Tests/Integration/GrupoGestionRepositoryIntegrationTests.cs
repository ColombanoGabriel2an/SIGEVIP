using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Grupos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Grupos;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class GrupoGestionRepositoryIntegrationTests
    {
        [TestMethod]
        public void Listar_GrupoPersistido_RecuperaDatosYCantidadPermisos()
        {
            DatosGrupoPrueba datos =
                CrearGrupoPrueba(
                    true,
                    "CLIENTE_CONSULTAR",
                    "VIAJE_CONSULTAR");

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                GrupoListadoDto grupo =
                    repository.Listar(
                            new GrupoFiltro(
                                datos.Codigo,
                                null))
                        .Single(
                            actual =>
                                actual.IdGrupo ==
                                datos.IdGrupo);

                Assert.AreEqual(
                    datos.Codigo,
                    grupo.Codigo);

                Assert.AreEqual(
                    datos.Nombre,
                    grupo.Nombre);

                Assert.AreEqual(
                    2,
                    grupo.CantidadPermisos);

                Assert.AreEqual(
                    0,
                    grupo.CantidadUsuarios);

                Assert.IsTrue(
                    grupo.Activo);
            }
            finally
            {
                EliminarDatosGrupo(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_FiltroTextoPorDescripcion_EncuentraGrupo()
        {
            DatosGrupoPrueba datos =
                CrearGrupoPrueba(
                    true,
                    "CLIENTE_CONSULTAR");

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                IReadOnlyCollection<GrupoListadoDto> resultados =
                    repository.Listar(
                        new GrupoFiltro(
                            datos.Sufijo,
                            null));

                Assert.IsTrue(
                    resultados.Any(
                        actual =>
                            actual.IdGrupo ==
                            datos.IdGrupo));
            }
            finally
            {
                EliminarDatosGrupo(
                    datos);
            }
        }

        [TestMethod]
        public void Listar_FiltroEstado_RecuperaGrupoInactivo()
        {
            DatosGrupoPrueba datos =
                CrearGrupoPrueba(
                    false,
                    "CLIENTE_CONSULTAR");

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                GrupoListadoDto grupo =
                    repository.Listar(
                            new GrupoFiltro(
                                datos.Codigo,
                                false))
                        .Single(
                            actual =>
                                actual.IdGrupo ==
                                datos.IdGrupo);

                Assert.IsFalse(
                    grupo.Activo);

                Assert.AreEqual(
                    "Inactivo",
                    grupo.Estado);
            }
            finally
            {
                EliminarDatosGrupo(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerDetallePorId_GrupoPersistido_RecuperaPermisosDirectos()
        {
            DatosGrupoPrueba datos =
                CrearGrupoPrueba(
                    true,
                    "CLIENTE_CONSULTAR",
                    "VIAJE_CONSULTAR");

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                GrupoDetalleDto detalle =
                    repository.ObtenerDetallePorId(
                        datos.IdGrupo);

                Assert.IsNotNull(
                    detalle);

                Assert.AreEqual(
                    datos.IdGrupo,
                    detalle.IdGrupo);

                Assert.AreEqual(
                    datos.Codigo,
                    detalle.Codigo);

                CollectionAssert.AreEquivalent(
                    datos.IdsPermisos.ToArray(),
                    detalle
                        .IdsPermisosDirectos
                        .ToArray());
            }
            finally
            {
                EliminarDatosGrupo(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerDetallePorId_GrupoInexistente_DevuelveNull()
        {
            GrupoGestionRepository repository =
                CrearRepository();

            GrupoDetalleDto detalle =
                repository.ObtenerDetallePorId(
                    int.MaxValue);

            Assert.IsNull(
                detalle);
        }

        [TestMethod]
        public void ObtenerPorId_GrupoPersistido_ReconstruyePermisosDirectos()
        {
            DatosGrupoPrueba datos =
                CrearGrupoPrueba(
                    true,
                    "CLIENTE_CONSULTAR",
                    "VIAJE_CONSULTAR");

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                Grupo grupo =
                    repository.ObtenerPorId(
                        datos.IdGrupo);

                Assert.IsNotNull(
                    grupo);

                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        "CLIENTE_CONSULTAR",
                        "VIAJE_CONSULTAR"
                    },
                    grupo.Componentes
                        .OfType<Permiso>()
                        .Select(
                            permiso =>
                                permiso.Codigo)
                        .ToArray());
            }
            finally
            {
                EliminarDatosGrupo(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerPorId_GrupoConHijo_ReconstruyeGrupoHijoDirecto()
        {
            DatosJerarquiaPrueba datos =
                CrearJerarquiaPrueba();

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                Grupo padre =
                    repository.ObtenerPorId(
                        datos.IdGrupoPadre);

                Grupo hijo =
                    padre.Componentes
                        .OfType<Grupo>()
                        .Single();

                Assert.AreEqual(
                    datos.IdGrupoHijo,
                    hijo.IdGrupo);

                Assert.AreEqual(
                    datos.CodigoGrupoHijo
                        .ToUpperInvariant(),
                    hijo.Codigo);
            }
            finally
            {
                EliminarJerarquiaPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ListarPermisosActivos_ExcluyeInactivosYMarcaSeleccionado()
        {
            DatosPermisoPrueba permisoInactivo =
                CrearPermisoPrueba(
                    false);

            try
            {
                int idSeleccionado =
                    ObtenerIdPermiso(
                        "CLIENTE_CONSULTAR");

                GrupoGestionRepository repository =
                    CrearRepository();

                IReadOnlyCollection<PermisoSeleccionGrupoDto>
                    permisos =
                        repository.ListarPermisosActivos(
                            new[]
                            {
                                idSeleccionado
                            });

                PermisoSeleccionGrupoDto seleccionado =
                    permisos.Single(
                        permiso =>
                            permiso.IdPermiso ==
                            idSeleccionado);

                Assert.IsTrue(
                    seleccionado.Seleccionado);

                Assert.IsFalse(
                    permisos.Any(
                        permiso =>
                            permiso.IdPermiso ==
                            permisoInactivo.IdPermiso));
            }
            finally
            {
                EliminarPermisoPrueba(
                    permisoInactivo);
            }
        }

        [TestMethod]
        public void ObtenerPermisosPorIds_RecuperaPermisosSolicitados()
        {
            int idCliente =
                ObtenerIdPermiso(
                    "CLIENTE_CONSULTAR");

            int idViaje =
                ObtenerIdPermiso(
                    "VIAJE_CONSULTAR");

            GrupoGestionRepository repository =
                CrearRepository();

            IReadOnlyCollection<Permiso> permisos =
                repository.ObtenerPermisosPorIds(
                    new[]
                    {
                        idCliente,
                        idViaje
                    });

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "CLIENTE_CONSULTAR",
                    "VIAJE_CONSULTAR"
                },
                permisos
                    .Select(
                        permiso =>
                            permiso.Codigo)
                    .ToArray());
        }

        [TestMethod]
        public void ExisteCodigo_GrupoPersistido_DevuelveTrue()
        {
            DatosGrupoPrueba datos =
                CrearGrupoPrueba(
                    true,
                    "CLIENTE_CONSULTAR");

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                Assert.IsTrue(
                    repository.ExisteCodigo(
                        datos.Codigo,
                        null));

                Assert.IsFalse(
                    repository.ExisteCodigo(
                        datos.Codigo,
                        datos.IdGrupo));
            }
            finally
            {
                EliminarDatosGrupo(
                    datos);
            }
        }

        [TestMethod]
        public void ExisteNombre_GrupoPersistido_DevuelveTrue()
        {
            DatosGrupoPrueba datos =
                CrearGrupoPrueba(
                    true,
                    "CLIENTE_CONSULTAR");

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                Assert.IsTrue(
                    repository.ExisteNombre(
                        datos.Nombre,
                        null));

                Assert.IsFalse(
                    repository.ExisteNombre(
                        datos.Nombre,
                        datos.IdGrupo));
            }
            finally
            {
                EliminarDatosGrupo(
                    datos);
            }
        }

        [TestMethod]
        public void ExistePermisoActivo_CatalogoVigente_DevuelveTrue()
        {
            GrupoGestionRepository repository =
                CrearRepository();

            Assert.IsTrue(
                repository.ExistePermisoActivo(
                    "GRUPO_GESTIONAR"));
        }

        [TestMethod]
        public void Insertar_DatosValidos_PersisteGrupoYPermisos()
        {
            int idCliente =
                ObtenerIdPermiso(
                    "CLIENTE_CONSULTAR");

            int idViaje =
                ObtenerIdPermiso(
                    "VIAJE_CONSULTAR");

            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            Grupo grupo =
                new Grupo(
                    0,
                    "ALTA_GRUPO_" + sufijo,
                    "Alta grupo " + sufijo,
                    "Grupo temporal de integración " +
                    sufijo);

            int idGrupo = 0;

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                idGrupo =
                    repository.Insertar(
                        grupo,
                        new[]
                        {
                            idCliente,
                            idViaje
                        });

                GrupoDetalleDto detalle =
                    repository.ObtenerDetallePorId(
                        idGrupo);

                Assert.IsNotNull(
                    detalle);

                Assert.AreEqual(
                    grupo.Codigo,
                    detalle.Codigo);

                Assert.AreEqual(
                    grupo.Nombre,
                    detalle.Nombre);

                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        idCliente,
                        idViaje
                    },
                    detalle
                        .IdsPermisosDirectos
                        .ToArray());
            }
            finally
            {
                EliminarGrupoPorId(
                    idGrupo);
            }
        }

        [TestMethod]
        public void Insertar_CodigoDuplicado_RechazaOperacion()
        {
            DatosGrupoPrueba existente =
                CrearGrupoPrueba(
                    true,
                    "CLIENTE_CONSULTAR");

            try
            {
                int idPermiso =
                    ObtenerIdPermiso(
                        "VIAJE_CONSULTAR");

                Grupo duplicado =
                    new Grupo(
                        0,
                        existente.Codigo,
                        "Otro nombre " +
                        existente.Sufijo,
                        "Descripción duplicada");

                GrupoGestionRepository repository =
                    CrearRepository();

                Assert.ThrowsException<ReglaNegocioException>(
                    () => repository.Insertar(
                        duplicado,
                        new[]
                        {
                            idPermiso
                        }));

                Assert.AreEqual(
                    1,
                    ContarGruposPorCodigo(
                        existente.Codigo));
            }
            finally
            {
                EliminarDatosGrupo(
                    existente);
            }
        }

        [TestMethod]
        public void Insertar_PermisoInactivo_RechazaYRestauraTransaccion()
        {
            DatosPermisoPrueba permiso =
                CrearPermisoPrueba(
                    false);

            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            string codigo =
                "GRUPO_ROLLBACK_" +
                sufijo;

            Grupo grupo =
                new Grupo(
                    0,
                    codigo,
                    "Grupo rollback " +
                    sufijo,
                    "Prueba de rollback");

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                Assert.ThrowsException<ReglaNegocioException>(
                    () => repository.Insertar(
                        grupo,
                        new[]
                        {
                            permiso.IdPermiso
                        }));

                Assert.AreEqual(
                    0,
                    ContarGruposPorCodigo(
                        codigo));
            }
            finally
            {
                EliminarPermisoPrueba(
                    permiso);
            }
        }

        [TestMethod]
        public void Actualizar_DatosValidos_ModificaDatosYReemplazaPermisos()
        {
            DatosGrupoPrueba datos =
                CrearGrupoPrueba(
                    true,
                    "CLIENTE_CONSULTAR");

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                Grupo grupo =
                    repository.ObtenerPorId(
                        datos.IdGrupo);

                int idViaje =
                    ObtenerIdPermiso(
                        "VIAJE_CONSULTAR");

                int idVisita =
                    ObtenerIdPermiso(
                        "VISITA_REGISTRAR");

                string nombreNuevo =
                    "Grupo modificado " +
                    datos.Sufijo;

                string descripcionNueva =
                    "Descripción modificada " +
                    datos.Sufijo;

                grupo.ActualizarDatos(
                    nombreNuevo,
                    descripcionNueva);

                repository.Actualizar(
                    grupo,
                    new[]
                    {
                        idViaje,
                        idVisita
                    });

                GrupoDetalleDto detalle =
                    repository.ObtenerDetallePorId(
                        datos.IdGrupo);

                Assert.AreEqual(
                    datos.Codigo,
                    detalle.Codigo);

                Assert.AreEqual(
                    nombreNuevo,
                    detalle.Nombre);

                Assert.AreEqual(
                    descripcionNueva,
                    detalle.Descripcion);

                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        idViaje,
                        idVisita
                    },
                    detalle
                        .IdsPermisosDirectos
                        .ToArray());
            }
            finally
            {
                EliminarDatosGrupo(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_GrupoConHijo_PreservaRelacionGrupoGrupo()
        {
            DatosJerarquiaPrueba datos =
                CrearJerarquiaPrueba();

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                Grupo padre =
                    repository.ObtenerPorId(
                        datos.IdGrupoPadre);

                int idViaje =
                    ObtenerIdPermiso(
                        "VIAJE_CONSULTAR");

                padre.ActualizarDatos(
                    "Padre modificado " +
                    datos.Sufijo,
                    "Descripción modificada " +
                    datos.Sufijo);

                repository.Actualizar(
                    padre,
                    new[]
                    {
                        idViaje
                    });

                Assert.IsTrue(
                    ExisteRelacionGrupoGrupo(
                        datos.IdGrupoPadre,
                        datos.IdGrupoHijo));

                Grupo persistido =
                    repository.ObtenerPorId(
                        datos.IdGrupoPadre);

                Assert.AreEqual(
                    datos.IdGrupoHijo,
                    persistido.Componentes
                        .OfType<Grupo>()
                        .Single()
                        .IdGrupo);
            }
            finally
            {
                EliminarJerarquiaPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void DesactivarYActivar_GrupoPersistido_ConservaCambios()
        {
            DatosGrupoPrueba datos =
                CrearGrupoPrueba(
                    true,
                    "CLIENTE_CONSULTAR");

            try
            {
                GrupoGestionRepository repository =
                    CrearRepository();

                repository.Desactivar(
                    datos.IdGrupo);

                Assert.IsFalse(
                    repository.ObtenerPorId(
                        datos.IdGrupo)
                        .Activo);

                repository.Activar(
                    datos.IdGrupo);

                Assert.IsTrue(
                    repository.ObtenerPorId(
                        datos.IdGrupo)
                        .Activo);
            }
            finally
            {
                EliminarDatosGrupo(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_GrupoInexistente_LanzaPersistenciaException()
        {
            int idPermiso =
                ObtenerIdPermiso(
                    "CLIENTE_CONSULTAR");

            Grupo grupo =
                new Grupo(
                    int.MaxValue,
                    "GRUPO_INEXISTENTE",
                    "Grupo inexistente",
                    "Prueba de grupo inexistente");

            GrupoGestionRepository repository =
                CrearRepository();

            Assert.ThrowsException<PersistenciaException>(
                () => repository.Actualizar(
                    grupo,
                    new[]
                    {
                        idPermiso
                    }));
        }

        private static GrupoGestionRepository CrearRepository()
        {
            return new GrupoGestionRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosGrupoPrueba CrearGrupoPrueba(
            bool activo,
            params string[] codigosPermisos)
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            DatosGrupoPrueba datos =
                new DatosGrupoPrueba(
                    sufijo,
                    "GRUPO_INTEGRACION_" +
                    sufijo,
                    "Grupo integración " +
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
                        datos.IdGrupo =
                            InsertarGrupo(
                                connection,
                                transaction,
                                datos.Codigo,
                                datos.Nombre,
                                datos.Descripcion,
                                activo);

                        foreach (
                            string codigoPermiso
                            in codigosPermisos)
                        {
                            int idPermiso =
                                ObtenerIdPermiso(
                                    connection,
                                    transaction,
                                    codigoPermiso);

                            InsertarGrupoPermiso(
                                connection,
                                transaction,
                                datos.IdGrupo,
                                idPermiso);

                            datos.IdsPermisos.Add(
                                idPermiso);
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

        private static DatosJerarquiaPrueba
            CrearJerarquiaPrueba()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            DatosJerarquiaPrueba datos =
                new DatosJerarquiaPrueba(
                    sufijo,
                    "PADRE_GESTION_" +
                    sufijo,
                    "HIJO_GESTION_" +
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
                        datos.IdGrupoPadre =
                            InsertarGrupo(
                                connection,
                                transaction,
                                datos.CodigoGrupoPadre,
                                datos.CodigoGrupoPadre,
                                "Grupo padre temporal",
                                true);

                        datos.IdGrupoHijo =
                            InsertarGrupo(
                                connection,
                                transaction,
                                datos.CodigoGrupoHijo,
                                datos.CodigoGrupoHijo,
                                "Grupo hijo temporal",
                                true);

                        int idPermiso =
                            ObtenerIdPermiso(
                                connection,
                                transaction,
                                "CLIENTE_CONSULTAR");

                        InsertarGrupoPermiso(
                            connection,
                            transaction,
                            datos.IdGrupoPadre,
                            idPermiso);

                        InsertarGrupoGrupo(
                            connection,
                            transaction,
                            datos.IdGrupoPadre,
                            datos.IdGrupoHijo);

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

        private static DatosPermisoPrueba
            CrearPermisoPrueba(
                bool activo)
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            string codigo =
                "PERMISO_INTEGRACION_" +
                sufijo;

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
                        codigo;

                command.Parameters.Add(
                    "@Nombre",
                    SqlDbType.NVarChar,
                    150).Value =
                        "Permiso integración " +
                        sufijo;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    500).Value =
                        "Permiso temporal de integración";

                command.Parameters.Add(
                    "@Activo",
                    SqlDbType.Bit).Value =
                        activo;

                connection.Open();

                return new DatosPermisoPrueba(
                    Convert.ToInt32(
                        command.ExecuteScalar()),
                    codigo);
            }
        }

        private static int InsertarGrupo(
            SqlConnection connection,
            SqlTransaction transaction,
            string codigo,
            string nombre,
            string descripcion,
            bool activo)
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

        private static void InsertarGrupoGrupo(
            SqlConnection connection,
            SqlTransaction transaction,
            int idGrupoPadre,
            int idGrupoHijo)
        {
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
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@IdGrupoPadre",
                    SqlDbType.Int).Value =
                        idGrupoPadre;

                command.Parameters.Add(
                    "@IdGrupoHijo",
                    SqlDbType.Int).Value =
                        idGrupoHijo;

                command.ExecuteNonQuery();
            }
        }

        private static int ObtenerIdPermiso(
            string codigo)
        {
            using (
                SqlConnection connection =
                    new SqlConnection(
                        ObtenerConnectionString()))
            {
                connection.Open();

                return ObtenerIdPermiso(
                    connection,
                    null,
                    codigo);
            }
        }

        private static int ObtenerIdPermiso(
            SqlConnection connection,
            SqlTransaction transaction,
            string codigo)
        {
            const string sql = @"
SELECT
    p.IdPermiso
FROM dbo.Permiso AS p
WHERE p.Codigo = @Codigo;";

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
                        codigo;

                object resultado =
                    command.ExecuteScalar();

                if (resultado == null ||
                    resultado == DBNull.Value)
                {
                    Assert.Fail(
                        "No existe el permiso " +
                        codigo +
                        ". Ejecute el seed de seguridad.");
                }

                return Convert.ToInt32(
                    resultado);
            }
        }

        private static bool ExisteRelacionGrupoGrupo(
            int idGrupoPadre,
            int idGrupoHijo)
        {
            const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.GrupoGrupo
            WHERE
                IdGrupoPadre = @IdGrupoPadre
                AND IdGrupoHijo = @IdGrupoHijo
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
                    "@IdGrupoPadre",
                    SqlDbType.Int).Value =
                        idGrupoPadre;

                command.Parameters.Add(
                    "@IdGrupoHijo",
                    SqlDbType.Int).Value =
                        idGrupoHijo;

                connection.Open();

                return Convert.ToBoolean(
                    command.ExecuteScalar());
            }
        }

        private static int ContarGruposPorCodigo(
            string codigo)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Grupo
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

        private static void EliminarDatosGrupo(
            DatosGrupoPrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            EliminarGrupoPorId(
                datos.IdGrupo);
        }

        private static void EliminarGrupoPorId(
            int idGrupo)
        {
            if (idGrupo <= 0)
            {
                return;
            }

            const string sql = @"
DELETE FROM dbo.UsuarioGrupo
WHERE IdGrupo = @IdGrupo;

DELETE FROM dbo.GrupoGrupo
WHERE
    IdGrupoPadre = @IdGrupo
    OR IdGrupoHijo = @IdGrupo;

DELETE FROM dbo.GrupoPermiso
WHERE IdGrupo = @IdGrupo;

DELETE FROM dbo.Grupo
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
                command.ExecuteNonQuery();
            }
        }

        private static void EliminarJerarquiaPrueba(
            DatosJerarquiaPrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            const string sql = @"
DELETE FROM dbo.UsuarioGrupo
WHERE IdGrupo IN
(
    @IdGrupoPadre,
    @IdGrupoHijo
);

DELETE FROM dbo.GrupoGrupo
WHERE
    IdGrupoPadre IN
    (
        @IdGrupoPadre,
        @IdGrupoHijo
    )
    OR IdGrupoHijo IN
    (
        @IdGrupoPadre,
        @IdGrupoHijo
    );

DELETE FROM dbo.GrupoPermiso
WHERE IdGrupo IN
(
    @IdGrupoPadre,
    @IdGrupoHijo
);

DELETE FROM dbo.Grupo
WHERE IdGrupo IN
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
                        datos.IdGrupoPadre;

                command.Parameters.Add(
                    "@IdGrupoHijo",
                    SqlDbType.Int).Value =
                        datos.IdGrupoHijo;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void EliminarPermisoPrueba(
            DatosPermisoPrueba datos)
        {
            if (datos == null)
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
                        datos.IdPermiso;

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

        private sealed class DatosGrupoPrueba
        {
            public DatosGrupoPrueba(
                string sufijo,
                string codigo,
                string nombre,
                string descripcion)
            {
                Sufijo = sufijo;
                Codigo = codigo;
                Nombre = nombre;
                Descripcion = descripcion;

                IdsPermisos =
                    new List<int>();
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

            public List<int> IdsPermisos
            {
                get;
                private set;
            }
        }

        private sealed class DatosJerarquiaPrueba
        {
            public DatosJerarquiaPrueba(
                string sufijo,
                string codigoGrupoPadre,
                string codigoGrupoHijo)
            {
                Sufijo = sufijo;
                CodigoGrupoPadre = codigoGrupoPadre;
                CodigoGrupoHijo = codigoGrupoHijo;
            }

            public int IdGrupoPadre
            {
                get;
                set;
            }

            public int IdGrupoHijo
            {
                get;
                set;
            }

            public string Sufijo
            {
                get;
                private set;
            }

            public string CodigoGrupoPadre
            {
                get;
                private set;
            }

            public string CodigoGrupoHijo
            {
                get;
                private set;
            }
        }

        private sealed class DatosPermisoPrueba
        {
            public DatosPermisoPrueba(
                int idPermiso,
                string codigo)
            {
                IdPermiso = idPermiso;
                Codigo = codigo;
            }

            public int IdPermiso
            {
                get;
                private set;
            }

            public string Codigo
            {
                get;
                private set;
            }
        }
    }
}
