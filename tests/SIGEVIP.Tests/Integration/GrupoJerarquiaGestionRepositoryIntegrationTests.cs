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
using SIGEVIP.Infrastructure.Grupos;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class GrupoJerarquiaGestionRepositoryIntegrationTests
    {
        [TestMethod]
        public void ObtenerDetallePorId_GrupoConHijos_RecuperaIdsGruposHijos()
        {
            DatosPrueba datos =
                CrearDatosBase();

            try
            {
                InsertarRelacion(
                    datos.IdGrupoPadre,
                    datos.IdGrupoHijoPrimero);

                InsertarRelacion(
                    datos.IdGrupoPadre,
                    datos.IdGrupoHijoSegundo);

                GrupoDetalleDto detalle =
                    CrearRepository()
                        .ObtenerDetallePorId(
                            datos.IdGrupoPadre);

                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        datos.IdGrupoHijoPrimero,
                        datos.IdGrupoHijoSegundo
                    },
                    detalle
                        .IdsGruposHijos
                        .ToArray());
            }
            finally
            {
                EliminarDatos(
                    datos);
            }
        }

        [TestMethod]
        public void ListarGruposActivos_ExcluyePadreEIncluyeInactivoSeleccionado()
        {
            DatosPrueba datos =
                CrearDatosBase();

            try
            {
                DesactivarGrupo(
                    datos.IdGrupoHijoSegundo);

                IReadOnlyCollection<GrupoSeleccionGrupoDto>
                    resultados =
                        CrearRepository()
                            .ListarGruposActivos(
                                datos.IdGrupoPadre,
                                new[]
                                {
                                    datos.IdGrupoHijoSegundo
                                });

                Assert.IsFalse(
                    resultados.Any(
                        grupo =>
                            grupo.IdGrupo ==
                                datos.IdGrupoPadre));

                GrupoSeleccionGrupoDto inactivo =
                    resultados.Single(
                        grupo =>
                            grupo.IdGrupo ==
                                datos.IdGrupoHijoSegundo);

                Assert.IsFalse(
                    inactivo.Activo);

                Assert.IsTrue(
                    inactivo.Seleccionado);
            }
            finally
            {
                EliminarDatos(
                    datos);
            }
        }

        [TestMethod]
        public void ListarGruposActivos_InactivoNoSeleccionado_LoExcluye()
        {
            DatosPrueba datos =
                CrearDatosBase();

            try
            {
                DesactivarGrupo(
                    datos.IdGrupoHijoSegundo);

                IReadOnlyCollection<GrupoSeleccionGrupoDto>
                    resultados =
                        CrearRepository()
                            .ListarGruposActivos(
                                datos.IdGrupoPadre,
                                new int[0]);

                Assert.IsFalse(
                    resultados.Any(
                        grupo =>
                            grupo.IdGrupo ==
                                datos.IdGrupoHijoSegundo));
            }
            finally
            {
                EliminarDatos(
                    datos);
            }
        }

        [TestMethod]
        public void Insertar_ConGrupoHijo_PersisteGrupoPermisoYGrupoGrupo()
        {
            DatosPrueba datos =
                CrearDatosBase();

            int idGrupoNuevo = 0;

            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            Grupo grupo =
                new Grupo(
                    0,
                    "PADRE_ALTA_" + sufijo,
                    "Padre alta " + sufijo,
                    "Grupo padre creado por prueba de integración.");

            try
            {
                idGrupoNuevo =
                    CrearRepository()
                        .Insertar(
                            grupo,
                            new[]
                            {
                                datos.IdPermiso
                            },
                            new[]
                            {
                                datos.IdGrupoHijoPrimero
                            });

                Assert.IsTrue(
                    ExisteRelacion(
                        idGrupoNuevo,
                        datos.IdGrupoHijoPrimero));

                Assert.IsTrue(
                    ExisteGrupoPermiso(
                        idGrupoNuevo,
                        datos.IdPermiso));

                GrupoDetalleDto detalle =
                    CrearRepository()
                        .ObtenerDetallePorId(
                            idGrupoNuevo);

                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        datos.IdGrupoHijoPrimero
                    },
                    detalle
                        .IdsGruposHijos
                        .ToArray());
            }
            finally
            {
                EliminarGrupoPorId(
                    idGrupoNuevo);

                EliminarDatos(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_ConNuevoHijo_ReemplazaRelacionAnterior()
        {
            DatosPrueba datos =
                CrearDatosBase();

            try
            {
                InsertarRelacion(
                    datos.IdGrupoPadre,
                    datos.IdGrupoHijoPrimero);

                Grupo padre =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdGrupoPadre);

                padre.ActualizarDatos(
                    "Padre actualizado " +
                    datos.Sufijo,
                    "Descripción actualizada.");

                CrearRepository()
                    .Actualizar(
                        padre,
                        new[]
                        {
                            datos.IdPermiso
                        },
                        new[]
                        {
                            datos.IdGrupoHijoSegundo
                        });

                Assert.IsFalse(
                    ExisteRelacion(
                        datos.IdGrupoPadre,
                        datos.IdGrupoHijoPrimero));

                Assert.IsTrue(
                    ExisteRelacion(
                        datos.IdGrupoPadre,
                        datos.IdGrupoHijoSegundo));
            }
            finally
            {
                EliminarDatos(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_SinGruposHijos_EliminaRelacionesExistentes()
        {
            DatosPrueba datos =
                CrearDatosBase();

            try
            {
                InsertarRelacion(
                    datos.IdGrupoPadre,
                    datos.IdGrupoHijoPrimero);

                InsertarRelacion(
                    datos.IdGrupoPadre,
                    datos.IdGrupoHijoSegundo);

                Grupo padre =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdGrupoPadre);

                CrearRepository()
                    .Actualizar(
                        padre,
                        new[]
                        {
                            datos.IdPermiso
                        },
                        new int[0]);

                Assert.AreEqual(
                    0,
                    ContarHijos(
                        datos.IdGrupoPadre));
            }
            finally
            {
                EliminarDatos(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_ConHijoInactivoNuevo_RechazaYConservaEstado()
        {
            DatosPrueba datos =
                CrearDatosBase();

            try
            {
                InsertarRelacion(
                    datos.IdGrupoPadre,
                    datos.IdGrupoHijoPrimero);

                DesactivarGrupo(
                    datos.IdGrupoHijoSegundo);

                Grupo padre =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdGrupoPadre);

                string nombreOriginal =
                    padre.Nombre;

                padre.ActualizarDatos(
                    "Nombre que debe revertirse",
                    "Descripción que debe revertirse.");

                Assert.ThrowsException<ReglaNegocioException>(
                    () =>
                        CrearRepository()
                            .Actualizar(
                                padre,
                                new[]
                                {
                                    datos.IdPermiso
                                },
                                new[]
                                {
                                    datos.IdGrupoHijoSegundo
                                }));

                Assert.AreEqual(
                    nombreOriginal,
                    ObtenerNombreGrupo(
                        datos.IdGrupoPadre));

                Assert.IsTrue(
                    ExisteRelacion(
                        datos.IdGrupoPadre,
                        datos.IdGrupoHijoPrimero));

                Assert.IsFalse(
                    ExisteRelacion(
                        datos.IdGrupoPadre,
                        datos.IdGrupoHijoSegundo));
            }
            finally
            {
                EliminarDatos(
                    datos);
            }
        }

        [TestMethod]
        public void Actualizar_QueProduceCicloIndirecto_RechazaYConservaEstado()
        {
            DatosPrueba datos =
                CrearDatosBase();

            int idGrupoIntermedio = 0;

            try
            {
                idGrupoIntermedio =
                    InsertarGrupoTemporal(
                        "INTERMEDIO_" +
                        datos.Sufijo,
                        true);

                InsertarGrupoPermiso(
                    idGrupoIntermedio,
                    datos.IdPermiso);

                InsertarRelacion(
                    datos.IdGrupoPadre,
                    datos.IdGrupoHijoPrimero);

                InsertarRelacion(
                    datos.IdGrupoHijoSegundo,
                    idGrupoIntermedio);

                InsertarRelacion(
                    idGrupoIntermedio,
                    datos.IdGrupoPadre);

                Grupo padre =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdGrupoPadre);

                string nombreOriginal =
                    padre.Nombre;

                padre.ActualizarDatos(
                    "Nombre que debe revertirse",
                    "Descripción que debe revertirse.");

                ReglaNegocioException excepcion =
                    Assert.ThrowsException<ReglaNegocioException>(
                        () =>
                            CrearRepository()
                                .Actualizar(
                                    padre,
                                    new[]
                                    {
                                        datos.IdPermiso
                                    },
                                    new[]
                                    {
                                        datos.IdGrupoHijoSegundo
                                    }));

                StringAssert.Contains(
                    excepcion.Message,
                    "ciclo");

                Assert.AreEqual(
                    nombreOriginal,
                    ObtenerNombreGrupo(
                        datos.IdGrupoPadre));

                Assert.IsTrue(
                    ExisteRelacion(
                        datos.IdGrupoPadre,
                        datos.IdGrupoHijoPrimero));

                Assert.IsFalse(
                    ExisteRelacion(
                        datos.IdGrupoPadre,
                        datos.IdGrupoHijoSegundo));
            }
            finally
            {
                EliminarGrupoPorId(
                    idGrupoIntermedio);

                EliminarDatos(
                    datos);
            }
        }

        private static GrupoGestionRepository CrearRepository()
        {
            return new GrupoGestionRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosPrueba CrearDatosBase()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            DatosPrueba datos =
                new DatosPrueba(
                    sufijo);

            try
            {
                datos.IdPermiso =
                    ObtenerIdPermiso(
                        "CLIENTE_CONSULTAR");

                datos.IdGrupoPadre =
                    InsertarGrupoTemporal(
                        "PADRE_" +
                        sufijo,
                        true);

                datos.IdGrupoHijoPrimero =
                    InsertarGrupoTemporal(
                        "HIJO_UNO_" +
                        sufijo,
                        true);

                datos.IdGrupoHijoSegundo =
                    InsertarGrupoTemporal(
                        "HIJO_DOS_" +
                        sufijo,
                        true);

                InsertarGrupoPermiso(
                    datos.IdGrupoPadre,
                    datos.IdPermiso);

                InsertarGrupoPermiso(
                    datos.IdGrupoHijoPrimero,
                    datos.IdPermiso);

                InsertarGrupoPermiso(
                    datos.IdGrupoHijoSegundo,
                    datos.IdPermiso);

                return datos;
            }
            catch
            {
                EliminarDatos(
                    datos);

                throw;
            }
        }

        private static int InsertarGrupoTemporal(
            string codigo,
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
                        "Grupo " +
                        codigo;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    500).Value =
                        "Grupo temporal para prueba de integración.";

                command.Parameters.Add(
                    "@Activo",
                    SqlDbType.Bit).Value =
                        activo;

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

        private static void InsertarGrupoPermiso(
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

            EjecutarRelacion(
                sql,
                idGrupo,
                idPermiso,
                "@IdGrupo",
                "@IdPermiso");
        }

        private static void InsertarRelacion(
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

            EjecutarRelacion(
                sql,
                idGrupoPadre,
                idGrupoHijo,
                "@IdGrupoPadre",
                "@IdGrupoHijo");
        }

        private static void EjecutarRelacion(
            string sql,
            int primerId,
            int segundoId,
            string primerParametro,
            string segundoParametro)
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
                    primerParametro,
                    SqlDbType.Int).Value =
                        primerId;

                command.Parameters.Add(
                    segundoParametro,
                    SqlDbType.Int).Value =
                        segundoId;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void DesactivarGrupo(
            int idGrupo)
        {
            const string sql = @"
UPDATE dbo.Grupo
SET Activo = 0
WHERE IdGrupo = @IdGrupo;";

            EjecutarConIdGrupo(
                sql,
                idGrupo);
        }

        private static bool ExisteRelacion(
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

        private static int ContarHijos(
            int idGrupoPadre)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.GrupoGrupo
WHERE IdGrupoPadre = @IdGrupo;";

            return Convert.ToInt32(
                EjecutarEscalarConIdGrupo(
                    sql,
                    idGrupoPadre));
        }

        private static string ObtenerNombreGrupo(
            int idGrupo)
        {
            const string sql = @"
SELECT Nombre
FROM dbo.Grupo
WHERE IdGrupo = @IdGrupo;";

            return Convert.ToString(
                EjecutarEscalarConIdGrupo(
                    sql,
                    idGrupo));
        }

        private static object EjecutarEscalarConIdGrupo(
            string sql,
            int idGrupo)
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
                    "@IdGrupo",
                    SqlDbType.Int).Value =
                        idGrupo;

                connection.Open();

                return command.ExecuteScalar();
            }
        }

        private static void EjecutarConIdGrupo(
            string sql,
            int idGrupo)
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
                    "@IdGrupo",
                    SqlDbType.Int).Value =
                        idGrupo;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void EliminarDatos(
            DatosPrueba datos)
        {
            if (datos == null)
            {
                return;
            }

            EliminarGrupoPorId(
                datos.IdGrupoPadre);

            EliminarGrupoPorId(
                datos.IdGrupoHijoPrimero);

            EliminarGrupoPorId(
                datos.IdGrupoHijoSegundo);
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

            EjecutarConIdGrupo(
                sql,
                idGrupo);
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
                string sufijo)
            {
                Sufijo = sufijo;
            }

            public string Sufijo
            {
                get;
                private set;
            }

            public int IdPermiso
            {
                get;
                set;
            }

            public int IdGrupoPadre
            {
                get;
                set;
            }

            public int IdGrupoHijoPrimero
            {
                get;
                set;
            }

            public int IdGrupoHijoSegundo
            {
                get;
                set;
            }
        }
    }
}
