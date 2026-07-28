using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Rendiciones;
using SIGEVIP.Infrastructure.Viajes;
using SIGEVIP.Infrastructure.Viaticos;

namespace SIGEVIP.Tests.Integration
{
    [TestClass]
    [TestCategory("IntegracionSQL")]
    public class RendicionRevisionAuditoriaRepositoryIntegrationTests
    {
        [TestMethod]
        public void ExcluirViatico_ConAuditoriaValida_PersisteCambioYEvento()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                Viatico viatico =
                    viaje.Viaticos.Single();

                DateTime fecha =
                    new DateTime(
                        2035,
                        6,
                        16,
                        10,
                        0,
                        0);

                viaje.ExcluirViatico(
                    viatico,
                    "Comprobante observado",
                    datos.IdUsuario,
                    fecha);

                repository.ExcluirViatico(
                    viaje,
                    viatico,
                    CrearAuditoria(
                        datos,
                        datos.IdUsuario,
                        "Exclusion",
                        "Viatico",
                        datos.IdViatico,
                        "exclusion valida"));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Viatico excluido =
                    recuperado.Viaticos.Single();

                Assert.AreEqual(
                    EstadoViatico.Excluido,
                    excluido.Estado);

                Assert.AreEqual(
                    "Comprobante observado",
                    excluido.MotivoExclusion);

                Assert.AreEqual(
                    datos.IdUsuario,
                    excluido.IdUsuarioExclusion);

                Assert.AreEqual(
                    fecha,
                    excluido.FechaExclusion);

                AfirmarAuditoriaPersistida(
                    datos,
                    "Exclusion",
                    "Viatico",
                    datos.IdViatico);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ExcluirViatico_ConActorInexistente_RevierteCambio()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                Viatico viatico =
                    viaje.Viaticos.Single();

                viaje.ExcluirViatico(
                    viatico,
                    "No debe persistirse",
                    datos.IdUsuario,
                    new DateTime(
                        2035,
                        6,
                        16,
                        10,
                        30,
                        0));

                Assert.ThrowsException<
                    PersistenciaException>(
                        () =>
                            repository.ExcluirViatico(
                                viaje,
                                viatico,
                                CrearAuditoria(
                                    datos,
                                    int.MaxValue,
                                    "Exclusion",
                                    "Viatico",
                                    datos.IdViatico,
                                    "exclusion invalida")));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Viatico vigente =
                    recuperado.Viaticos.Single();

                Assert.AreEqual(
                    EstadoViatico.Vigente,
                    vigente.Estado);

                Assert.IsNull(
                    vigente.MotivoExclusion);

                Assert.IsFalse(
                    vigente.IdUsuarioExclusion.HasValue);

                Assert.IsFalse(
                    vigente.FechaExclusion.HasValue);

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos,
                        "Exclusion",
                        "Viatico",
                        datos.IdViatico));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ReactivarViatico_ConAuditoriaValida_PersisteCambioYEvento()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    PrepararViaticoExcluido(
                        repository,
                        datos);

                Viatico viatico =
                    viaje.Viaticos.Single();

                DateTime fecha =
                    new DateTime(
                        2035,
                        6,
                        16,
                        12,
                        0,
                        0);

                viaje.ReactivarViatico(
                    viatico,
                    datos.IdUsuario,
                    fecha);

                repository.ReactivarViatico(
                    viaje,
                    viatico,
                    CrearAuditoria(
                        datos,
                        datos.IdUsuario,
                        "Reactivacion",
                        "Viatico",
                        datos.IdViatico,
                        "reactivacion valida"));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Viatico reactivado =
                    recuperado.Viaticos.Single();

                Assert.AreEqual(
                    EstadoViatico.Vigente,
                    reactivado.Estado);

                Assert.AreEqual(
                    datos.IdUsuario,
                    reactivado.IdUsuarioReactivacion);

                Assert.AreEqual(
                    fecha,
                    reactivado.FechaReactivacion);

                AfirmarAuditoriaPersistida(
                    datos,
                    "Reactivacion",
                    "Viatico",
                    datos.IdViatico);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void ReactivarViatico_ConActorInexistente_RevierteCambio()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    PrepararViaticoExcluido(
                        repository,
                        datos);

                Viatico viatico =
                    viaje.Viaticos.Single();

                viaje.ReactivarViatico(
                    viatico,
                    datos.IdUsuario,
                    new DateTime(
                        2035,
                        6,
                        16,
                        12,
                        30,
                        0));

                Assert.ThrowsException<
                    PersistenciaException>(
                        () =>
                            repository.ReactivarViatico(
                                viaje,
                                viatico,
                                CrearAuditoria(
                                    datos,
                                    int.MaxValue,
                                    "Reactivacion",
                                    "Viatico",
                                    datos.IdViatico,
                                    "reactivacion invalida")));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Viatico excluido =
                    recuperado.Viaticos.Single();

                Assert.AreEqual(
                    EstadoViatico.Excluido,
                    excluido.Estado);

                Assert.IsFalse(
                    excluido.IdUsuarioReactivacion.HasValue);

                Assert.IsFalse(
                    excluido.FechaReactivacion.HasValue);

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos,
                        "Reactivacion",
                        "Viatico",
                        datos.IdViatico));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void AjustarMontoAnticipado_ConAuditoriaValida_PersisteCambioYEvento()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                viaje.AjustarMontoAnticipado(
                    1750m);

                repository.AjustarMontoAnticipado(
                    viaje,
                    CrearAuditoria(
                        datos,
                        datos.IdUsuario,
                        "AjusteAnticipo",
                        "Viaje",
                        datos.IdViaje,
                        "ajuste valido"));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    1750m,
                    recuperado.MontoAnticipado);

                AfirmarAuditoriaPersistida(
                    datos,
                    "AjusteAnticipo",
                    "Viaje",
                    datos.IdViaje);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void AjustarMontoAnticipado_ConActorInexistente_RevierteCambio()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                viaje.AjustarMontoAnticipado(
                    1750m);

                Assert.ThrowsException<
                    PersistenciaException>(
                        () =>
                            repository.AjustarMontoAnticipado(
                                viaje,
                                CrearAuditoria(
                                    datos,
                                    int.MaxValue,
                                    "AjusteAnticipo",
                                    "Viaje",
                                    datos.IdViaje,
                                    "ajuste invalido")));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    datos.MontoAnticipado,
                    recuperado.MontoAnticipado);

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos,
                        "AjusteAnticipo",
                        "Viaje",
                        datos.IdViaje));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Aprobar_ConAuditoriaValida_PersisteCambioYEvento()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                DateTime fecha =
                    new DateTime(
                        2035,
                        6,
                        16,
                        14,
                        0,
                        0);

                viaje.Aprobar(
                    datos.IdUsuario,
                    fecha);

                repository.Aprobar(
                    viaje,
                    CrearAuditoria(
                        datos,
                        datos.IdUsuario,
                        "Aprobacion",
                        "Viaje",
                        datos.IdViaje,
                        "aprobacion valida"));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    EstadoViaje.Aprobado,
                    recuperado.EstadoActual);

                Assert.AreEqual(
                    datos.IdUsuario,
                    recuperado.IdUsuarioAprobador);

                Assert.AreEqual(
                    fecha,
                    recuperado.FechaAprobacion);

                AfirmarAuditoriaPersistida(
                    datos,
                    "Aprobacion",
                    "Viaje",
                    datos.IdViaje);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Aprobar_ConActorInexistente_RevierteCambio()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                viaje.Aprobar(
                    datos.IdUsuario,
                    new DateTime(
                        2035,
                        6,
                        16,
                        14,
                        30,
                        0));

                Assert.ThrowsException<
                    PersistenciaException>(
                        () =>
                            repository.Aprobar(
                                viaje,
                                CrearAuditoria(
                                    datos,
                                    int.MaxValue,
                                    "Aprobacion",
                                    "Viaje",
                                    datos.IdViaje,
                                    "aprobacion invalida")));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    EstadoViaje.EnRendicion,
                    recuperado.EstadoActual);

                Assert.IsFalse(
                    recuperado.IdUsuarioAprobador.HasValue);

                Assert.IsFalse(
                    recuperado.FechaAprobacion.HasValue);

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos,
                        "Aprobacion",
                        "Viaje",
                        datos.IdViaje));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Cancelar_ConAuditoriaValida_PersisteCambioYEvento()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                DateTime fecha =
                    new DateTime(
                        2035,
                        6,
                        16,
                        15,
                        0,
                        0);

                viaje.Cancelar(
                    "Rendición cancelada",
                    datos.IdUsuario,
                    fecha);

                repository.Cancelar(
                    viaje,
                    CrearAuditoria(
                        datos,
                        datos.IdUsuario,
                        "Cancelacion",
                        "Viaje",
                        datos.IdViaje,
                        "cancelacion valida"));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    EstadoViaje.Cancelado,
                    recuperado.EstadoActual);

                Assert.AreEqual(
                    "Rendición cancelada",
                    recuperado.MotivoCancelacion);

                Assert.AreEqual(
                    datos.IdUsuario,
                    recuperado.IdUsuarioCancelacion);

                Assert.AreEqual(
                    fecha,
                    recuperado.FechaCancelacion);

                AfirmarAuditoriaPersistida(
                    datos,
                    "Cancelacion",
                    "Viaje",
                    datos.IdViaje);
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        [TestMethod]
        public void Cancelar_ConActorInexistente_RevierteCambio()
        {
            DatosPrueba datos =
                CrearDatosConViatico();

            try
            {
                RendicionRepository repository =
                    CrearRendicionRepository();

                Viaje viaje =
                    EnviarARendicion(
                        repository,
                        datos);

                viaje.Cancelar(
                    "No debe persistirse",
                    datos.IdUsuario,
                    new DateTime(
                        2035,
                        6,
                        16,
                        15,
                        30,
                        0));

                Assert.ThrowsException<
                    PersistenciaException>(
                        () =>
                            repository.Cancelar(
                                viaje,
                                CrearAuditoria(
                                    datos,
                                    int.MaxValue,
                                    "Cancelacion",
                                    "Viaje",
                                    datos.IdViaje,
                                    "cancelacion invalida")));

                Viaje recuperado =
                    repository.ObtenerPorId(
                        datos.IdViaje);

                Assert.AreEqual(
                    EstadoViaje.EnRendicion,
                    recuperado.EstadoActual);

                Assert.IsNull(
                    recuperado.MotivoCancelacion);

                Assert.IsFalse(
                    recuperado.IdUsuarioCancelacion.HasValue);

                Assert.IsFalse(
                    recuperado.FechaCancelacion.HasValue);

                Assert.AreEqual(
                    0,
                    ContarAuditorias(
                        datos,
                        "Cancelacion",
                        "Viaje",
                        datos.IdViaje));
            }
            finally
            {
                EliminarDatosPrueba(
                    datos);
            }
        }

        private static Viaje EnviarARendicion(
            RendicionRepository repository,
            DatosPrueba datos)
        {
            Viaje viaje =
                repository.ObtenerPorId(
                    datos.IdViaje);

            viaje.EnviarARendicion(
                datos.IdUsuario,
                new DateTime(
                    2035,
                    6,
                    16,
                    9,
                    0,
                    0));

            repository.Enviar(
                viaje);

            return repository.ObtenerPorId(
                datos.IdViaje);
        }

        private static Viaje PrepararViaticoExcluido(
            RendicionRepository repository,
            DatosPrueba datos)
        {
            Viaje viaje =
                EnviarARendicion(
                    repository,
                    datos);

            Viatico viatico =
                viaje.Viaticos.Single();

            viaje.ExcluirViatico(
                viatico,
                "Preparación de reactivación",
                datos.IdUsuario,
                new DateTime(
                    2035,
                    6,
                    16,
                    11,
                    0,
                    0));

            repository.ExcluirViatico(
                viaje,
                viatico);

            return repository.ObtenerPorId(
                datos.IdViaje);
        }

        private static AuditoriaRegistro CrearAuditoria(
            DatosPrueba datos,
            int idUsuario,
            string accion,
            string entidad,
            int idEntidad,
            string descripcion)
        {
            return new AuditoriaRegistro(
                idUsuario,
                idUsuario == datos.IdUsuario
                    ? "usuario_integracion_rendicion"
                    : "actor_rendicion_inexistente",
                "Rendiciones",
                accion,
                entidad,
                idEntidad,
                datos.Marca +
                    " " +
                    descripcion);
        }

        private static void AfirmarAuditoriaPersistida(
            DatosPrueba datos,
            string accion,
            string entidad,
            int idEntidad)
        {
            const string sql = @"
SELECT
    COUNT(*),
    MIN(IdUsuario)
FROM dbo.Auditoria
WHERE
    Modulo = N'Rendiciones'
    AND Accion = @Accion
    AND Entidad = @Entidad
    AND IdEntidad = @IdEntidad
    AND Descripcion LIKE @Marca;";

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
                ConfigurarParametrosAuditoria(
                    command.Parameters,
                    datos,
                    accion,
                    entidad,
                    idEntidad);

                connection.Open();

                using (
                    SqlDataReader reader =
                        command.ExecuteReader(
                            CommandBehavior.SingleRow))
                {
                    Assert.IsTrue(
                        reader.Read());

                    Assert.AreEqual(
                        1,
                        reader.GetInt32(
                            0));

                    Assert.AreEqual(
                        datos.IdUsuario,
                        reader.GetInt32(
                            1));
                }
            }
        }

        private static int ContarAuditorias(
            DatosPrueba datos,
            string accion,
            string entidad,
            int idEntidad)
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.Auditoria
WHERE
    Modulo = N'Rendiciones'
    AND Accion = @Accion
    AND Entidad = @Entidad
    AND IdEntidad = @IdEntidad
    AND Descripcion LIKE @Marca;";

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
                ConfigurarParametrosAuditoria(
                    command.Parameters,
                    datos,
                    accion,
                    entidad,
                    idEntidad);

                connection.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar());
            }
        }

        private static void ConfigurarParametrosAuditoria(
            SqlParameterCollection parametros,
            DatosPrueba datos,
            string accion,
            string entidad,
            int idEntidad)
        {
            parametros.Add(
                "@Accion",
                SqlDbType.NVarChar,
                50).Value =
                    accion;

            parametros.Add(
                "@Entidad",
                SqlDbType.NVarChar,
                100).Value =
                    entidad;

            parametros.Add(
                "@IdEntidad",
                SqlDbType.Int).Value =
                    idEntidad;

            parametros.Add(
                "@Marca",
                SqlDbType.NVarChar,
                1000).Value =
                    "%" +
                    datos.Marca +
                    "%";
        }

        private static DatosPrueba CrearDatosConViatico()
        {
            string sufijo =
                Guid.NewGuid()
                    .ToString("N");

            var datos =
                new DatosPrueba(
                    new DateTime(
                        2035,
                        6,
                        10),
                    new DateTime(
                        2035,
                        6,
                        15),
                    1000m,
                    1400m);

            datos.Marca =
                "RENDICION_REVISION_AUDIT_TEST_" +
                sufijo;

            datos.IdPersona =
                InsertarPersona(
                    "Participante",
                    "Audit" +
                        sufijo.Substring(
                            0,
                            8),
                    "revision_audit_" +
                        sufijo +
                        "@sigevip.test");

            datos.IdUsuario =
                ObtenerIdUsuarioActivo();

            var viaje =
                new Viaje(
                    0,
                    datos.FechaInicio,
                    datos.FechaFin,
                    datos.Marca,
                    TipoViaje.Desplazamiento,
                    datos.MontoAnticipado);

            viaje.ReemplazarParticipantes(
                new[]
                {
                    datos.CrearPersona()
                });

            datos.IdViaje =
                CrearViajeRepository()
                    .Insertar(
                        viaje);

            var viajePersistido =
                new Viaje(
                    datos.IdViaje,
                    datos.FechaInicio,
                    datos.FechaFin,
                    datos.Marca,
                    TipoViaje.Desplazamiento,
                    datos.MontoAnticipado);

            viajePersistido.ReemplazarParticipantes(
                new[]
                {
                    datos.CrearPersona()
                });

            var viatico =
                new Viatico(
                    0,
                    datos.FechaInicio.AddDays(
                        1),
                    CategoriaGasto.Alimentacion,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    datos.MontoViatico,
                    datos.Marca,
                    null);

            viajePersistido.AgregarViatico(
                viatico);

            datos.IdViatico =
                CrearViaticoRepository()
                    .Insertar(
                        viatico);

            return datos;
        }

        private static RendicionRepository
            CrearRendicionRepository()
        {
            return new RendicionRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static ViajeRepository
            CrearViajeRepository()
        {
            return new ViajeRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static ViaticoRepository
            CrearViaticoRepository()
        {
            return new ViaticoRepository(
                new SqlConnectionFactory(
                    ObtenerConnectionString()));
        }

        private static int ObtenerIdUsuarioActivo()
        {
            const string sql = @"
SELECT TOP (1)
    IdUsuario
FROM dbo.Usuario
WHERE Activo = 1
ORDER BY IdUsuario;";

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
                connection.Open();

                object resultado =
                    command.ExecuteScalar();

                if (resultado == null ||
                    resultado == DBNull.Value)
                {
                    Assert.Fail(
                        "La base de integración debe contener al menos un usuario activo.");
                }

                return Convert.ToInt32(
                    resultado);
            }
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

        private static void EliminarDatosPrueba(
            DatosPrueba datos)
        {
            if (datos == null)
            {
                return;
            }

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
                        if (datos.IdViatico > 0)
                        {
                            EjecutarEliminacion(
                                connection,
                                transaction,
                                @"
DELETE FROM dbo.Auditoria
WHERE
    Modulo = N'Rendiciones'
    AND Entidad = N'Viatico'
    AND IdEntidad = @Valor;",
                                datos.IdViatico);
                        }

                        if (datos.IdViaje > 0)
                        {
                            EjecutarEliminacion(
                                connection,
                                transaction,
                                @"
DELETE FROM dbo.Auditoria
WHERE
    Modulo = N'Rendiciones'
    AND Entidad = N'Viaje'
    AND IdEntidad = @Valor;

DELETE FROM dbo.Comprobante
WHERE IdViatico IN
(
    SELECT IdViatico
    FROM dbo.Viatico
    WHERE IdViaje = @Valor
);

DELETE FROM dbo.Viatico
WHERE IdViaje = @Valor;

DELETE FROM dbo.VisitaCliente
WHERE IdVisita IN
(
    SELECT IdVisita
    FROM dbo.Visita
    WHERE IdViaje = @Valor
);

DELETE FROM dbo.Visita
WHERE IdViaje = @Valor;

DELETE FROM dbo.ViajeParticipante
WHERE IdViaje = @Valor;

DELETE FROM dbo.Viaje
WHERE IdViaje = @Valor;",
                                datos.IdViaje);
                        }

                        if (datos.IdPersona > 0)
                        {
                            EjecutarEliminacion(
                                connection,
                                transaction,
                                @"
DELETE FROM dbo.ViajeParticipante
WHERE IdPersona = @Valor;

DELETE FROM dbo.Persona
WHERE IdPersona = @Valor;",
                                datos.IdPersona);
                        }

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

        private static void EjecutarEliminacion(
            SqlConnection connection,
            SqlTransaction transaction,
            string sql,
            int valor)
        {
            using (
                SqlCommand command =
                    new SqlCommand(
                        sql,
                        connection,
                        transaction))
            {
                command.Parameters.Add(
                    "@Valor",
                    SqlDbType.Int).Value =
                        valor;

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
                    "No se encontró la cadena SIGEVIP en App.config.");
            }

            return settings.ConnectionString;
        }

        private sealed class DatosPrueba
        {
            public DatosPrueba(
                DateTime fechaInicio,
                DateTime fechaFin,
                decimal montoAnticipado,
                decimal montoViatico)
            {
                FechaInicio = fechaInicio;
                FechaFin = fechaFin;
                MontoAnticipado =
                    montoAnticipado;
                MontoViatico =
                    montoViatico;
            }

            public int IdPersona { get; set; }

            public int IdUsuario { get; set; }

            public int IdViaje { get; set; }

            public int IdViatico { get; set; }

            public string Marca { get; set; }

            public DateTime FechaInicio
            {
                get;
                private set;
            }

            public DateTime FechaFin
            {
                get;
                private set;
            }

            public decimal MontoAnticipado
            {
                get;
                private set;
            }

            public decimal MontoViatico
            {
                get;
                private set;
            }

            public Persona CrearPersona()
            {
                return new Persona(
                    IdPersona,
                    "Participante",
                    "Auditoria",
                    "participante.auditoria@sigevip.test");
            }
        }
    }
}
