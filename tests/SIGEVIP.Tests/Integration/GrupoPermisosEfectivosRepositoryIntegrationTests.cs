using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Grupos;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Grupos;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class GrupoPermisosEfectivosRepositoryIntegrationTests
    {
        [TestMethod]
        public void ObtenerPermisosEfectivosVistaPrevia_DirectoYHeredadoDuplicado_PriorizaDirecto()
        {
            DatosPrueba datos =
                CrearDatosBase();

            try
            {
                InsertarGrupoPermiso(
                    datos.IdGrupoHijo,
                    datos.IdPermisoDirecto);

                IReadOnlyCollection<PermisoEfectivoGrupoDto>
                    resultado =
                        CrearRepository()
                            .ObtenerPermisosEfectivosVistaPrevia(
                                new[]
                                {
                                    datos.IdPermisoDirecto
                                },
                                new[]
                                {
                                    datos.IdGrupoHijo
                                });

                PermisoEfectivoGrupoDto permiso =
                    resultado.Single(
                        item =>
                            item.IdPermiso ==
                                datos.IdPermisoDirecto);

                Assert.IsTrue(
                    permiso.Directo);

                Assert.AreEqual(
                    1,
                    resultado.Count(
                        item =>
                            item.IdPermiso ==
                                datos.IdPermisoDirecto));
            }
            finally
            {
                EliminarDatos(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerPermisosEfectivosVistaPrevia_JerarquiaMultinivel_RecuperaPermisoHeredado()
        {
            DatosPrueba datos =
                CrearDatosBase();

            try
            {
                InsertarRelacion(
                    datos.IdGrupoHijo,
                    datos.IdGrupoNieto);

                InsertarGrupoPermiso(
                    datos.IdGrupoNieto,
                    datos.IdPermisoHeredado);

                IReadOnlyCollection<PermisoEfectivoGrupoDto>
                    resultado =
                        CrearRepository()
                            .ObtenerPermisosEfectivosVistaPrevia(
                                new[]
                                {
                                    datos.IdPermisoDirecto
                                },
                                new[]
                                {
                                    datos.IdGrupoHijo
                                });

                Assert.IsTrue(
                    resultado.Any(
                        permiso =>
                            permiso.IdPermiso ==
                                datos.IdPermisoDirecto
                            && permiso.Directo));

                Assert.IsTrue(
                    resultado.Any(
                        permiso =>
                            permiso.IdPermiso ==
                                datos.IdPermisoHeredado
                            && !permiso.Directo));
            }
            finally
            {
                EliminarDatos(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerPermisosEfectivosVistaPrevia_GrupoOPermisoInactivo_NoLosIncluye()
        {
            DatosPrueba datos =
                CrearDatosBase();

            try
            {
                InsertarGrupoPermiso(
                    datos.IdGrupoHijo,
                    datos.IdPermisoHeredado);

                DesactivarGrupo(
                    datos.IdGrupoHijo);

                DesactivarPermiso(
                    datos.IdPermisoDirecto);

                IReadOnlyCollection<PermisoEfectivoGrupoDto>
                    resultado =
                        CrearRepository()
                            .ObtenerPermisosEfectivosVistaPrevia(
                                new[]
                                {
                                    datos.IdPermisoDirecto
                                },
                                new[]
                                {
                                    datos.IdGrupoHijo
                                });

                Assert.AreEqual(
                    0,
                    resultado.Count);
            }
            finally
            {
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
                new DatosPrueba();

            try
            {
                datos.IdGrupoHijo =
                    InsertarGrupo(
                        "VISTA_HIJO_" +
                        sufijo);

                datos.IdGrupoNieto =
                    InsertarGrupo(
                        "VISTA_NIETO_" +
                        sufijo);

                datos.IdPermisoDirecto =
                    InsertarPermiso(
                        "VISTA_DIRECTO_" +
                        sufijo);

                datos.IdPermisoHeredado =
                    InsertarPermiso(
                        "VISTA_HEREDADO_" +
                        sufijo);

                return datos;
            }
            catch
            {
                EliminarDatos(
                    datos);

                throw;
            }
        }

        private static int InsertarGrupo(
            string codigo)
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
                        "Grupo temporal para vista previa.";

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static int InsertarPermiso(
            string codigo)
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
                    "@Codigo",
                    SqlDbType.NVarChar,
                    100).Value =
                        codigo;

                command.Parameters.Add(
                    "@Nombre",
                    SqlDbType.NVarChar,
                    150).Value =
                        "Permiso " +
                        codigo;

                command.Parameters.Add(
                    "@Descripcion",
                    SqlDbType.NVarChar,
                    500).Value =
                        "Permiso temporal para vista previa.";

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
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
                "@IdGrupo",
                idGrupo,
                "@IdPermiso",
                idPermiso);
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
                "@IdGrupoPadre",
                idGrupoPadre,
                "@IdGrupoHijo",
                idGrupoHijo);
        }

        private static void EjecutarRelacion(
            string sql,
            string primerParametro,
            int primerId,
            string segundoParametro,
            int segundoId)
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
WHERE IdGrupo = @Id;";

            EjecutarActualizacion(
                sql,
                idGrupo);
        }

        private static void DesactivarPermiso(
            int idPermiso)
        {
            const string sql = @"
UPDATE dbo.Permiso
SET Activo = 0
WHERE IdPermiso = @Id;";

            EjecutarActualizacion(
                sql,
                idPermiso);
        }

        private static void EjecutarActualizacion(
            string sql,
            int id)
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
                    "@Id",
                    SqlDbType.Int).Value =
                        id;

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

            const string sql = @"
DELETE FROM dbo.UsuarioGrupo
WHERE IdGrupo IN
(
    @IdGrupoHijo,
    @IdGrupoNieto
);

DELETE FROM dbo.GrupoGrupo
WHERE
    IdGrupoPadre IN
    (
        @IdGrupoHijo,
        @IdGrupoNieto
    )
    OR IdGrupoHijo IN
    (
        @IdGrupoHijo,
        @IdGrupoNieto
    );

DELETE FROM dbo.GrupoPermiso
WHERE
    IdGrupo IN
    (
        @IdGrupoHijo,
        @IdGrupoNieto
    )
    OR IdPermiso IN
    (
        @IdPermisoDirecto,
        @IdPermisoHeredado
    );

DELETE FROM dbo.Grupo
WHERE IdGrupo IN
(
    @IdGrupoHijo,
    @IdGrupoNieto
);

DELETE FROM dbo.Permiso
WHERE IdPermiso IN
(
    @IdPermisoDirecto,
    @IdPermisoHeredado
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
                    "@IdGrupoHijo",
                    SqlDbType.Int).Value =
                        datos.IdGrupoHijo;

                command.Parameters.Add(
                    "@IdGrupoNieto",
                    SqlDbType.Int).Value =
                        datos.IdGrupoNieto;

                command.Parameters.Add(
                    "@IdPermisoDirecto",
                    SqlDbType.Int).Value =
                        datos.IdPermisoDirecto;

                command.Parameters.Add(
                    "@IdPermisoHeredado",
                    SqlDbType.Int).Value =
                        datos.IdPermisoHeredado;

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
            public int IdGrupoHijo
            {
                get;
                set;
            }

            public int IdGrupoNieto
            {
                get;
                set;
            }

            public int IdPermisoDirecto
            {
                get;
                set;
            }

            public int IdPermisoHeredado
            {
                get;
                set;
            }
        }
    }
}
