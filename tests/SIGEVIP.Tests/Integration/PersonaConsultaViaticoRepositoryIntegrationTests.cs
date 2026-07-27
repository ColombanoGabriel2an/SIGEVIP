using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Viaticos;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class PersonaConsultaViaticoRepositoryIntegrationTests
    {
        [TestMethod]
        public void ListarActivas_DevuelvePersonaActiva()
        {
            DatosPersonaPrueba datos =
                CrearDatosPrueba();

            try
            {
                PagadorSeleccionDto resultado =
                    CrearRepository()
                        .ListarActivas()
                        .SingleOrDefault(
                            persona =>
                                persona.IdPersona ==
                                datos.IdPersona);

                Assert.IsNotNull(
                    resultado);

                Assert.IsTrue(
                    resultado.Activo);

                Assert.AreEqual(
                    datos.Nombre +
                    " " +
                    datos.Apellido,
                    resultado.NombreCompleto);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ListarActivas_NoDevuelvePersonaInactiva()
        {
            DatosPersonaPrueba datos =
                CrearDatosPrueba();

            try
            {
                DesactivarPersona(
                    datos.IdPersona);

                bool encontrada =
                    CrearRepository()
                        .ListarActivas()
                        .Any(
                            persona =>
                                persona.IdPersona ==
                                datos.IdPersona);

                Assert.IsFalse(
                    encontrada);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerPorId_PersonaInactiva_ConservaEstado()
        {
            DatosPersonaPrueba datos =
                CrearDatosPrueba();

            try
            {
                DesactivarPersona(
                    datos.IdPersona);

                Persona resultado =
                    CrearRepository()
                        .ObtenerPorId(
                            datos.IdPersona);

                Assert.IsNotNull(
                    resultado);

                Assert.AreEqual(
                    datos.IdPersona,
                    resultado.IdPersona);

                Assert.IsFalse(
                    resultado.Activo);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ObtenerPorId_PersonaInexistente_DevuelveNull()
        {
            Persona resultado =
                CrearRepository()
                    .ObtenerPorId(
                        int.MaxValue);

            Assert.IsNull(
                resultado);
        }

        [TestMethod]
        public void ObtenerPorId_IdInvalido_LanzaExcepcion()
        {
            Assert.ThrowsException<
                ArgumentOutOfRangeException>(
                    () => CrearRepository()
                        .ObtenerPorId(
                            0));
        }

        private static PersonaConsultaViaticoRepository
            CrearRepository()
        {
            return new PersonaConsultaViaticoRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static DatosPersonaPrueba
            CrearDatosPrueba()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            var datos =
                new DatosPersonaPrueba(
                    "Pagador",
                    "Viatico" +
                        sufijo.Substring(
                            0,
                            8),
                    "pagador_" +
                        sufijo +
                        "@sigevip.test");

            datos.IdPersona =
                InsertarPersona(
                    datos.Nombre,
                    datos.Apellido,
                    datos.Email);

            return datos;
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

        private static void DesactivarPersona(
            int idPersona)
        {
            const string sql = @"
UPDATE dbo.Persona
SET Activo = 0
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
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        idPersona;

                connection.Open();

                command.ExecuteNonQuery();
            }
        }

        private static void EliminarDatosPrueba(
            DatosPersonaPrueba datos)
        {
            if (datos == null ||
                datos.IdPersona <= 0)
            {
                return;
            }

            const string sql = @"
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
                    "@IdPersona",
                    SqlDbType.Int).Value =
                        datos.IdPersona;

                connection.Open();

                command.ExecuteNonQuery();
            }
        }

        private static string ObtenerConnectionString()
        {
            ConnectionStringSettings settings =
                ConfigurationManager
                    .ConnectionStrings["SIGEVIP"];

            if (settings == null ||
                string.IsNullOrWhiteSpace(
                    settings.ConnectionString))
            {
                Assert.Fail(
                    "No se encontró la cadena SIGEVIP en App.config.");
            }

            return settings.ConnectionString;
        }

        private sealed class DatosPersonaPrueba
        {
            public DatosPersonaPrueba(
                string nombre,
                string apellido,
                string email)
            {
                Nombre = nombre;
                Apellido = apellido;
                Email = email;
            }

            public int IdPersona { get; set; }

            public string Nombre
            {
                get;
                private set;
            }

            public string Apellido
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
