using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class ViajeRendicionTests
    {
        [TestMethod]
        public void ModificarViatico_EnAbierto_ActualizaDatos()
        {
            Viaje viaje =
                CrearViaje();

            Viatico viatico =
                CrearViatico(
                    1,
                    1000m);

            viaje.AgregarViatico(
                viatico);

            Persona pagador =
                CrearPersona();

            viaje.ModificarViatico(
                viatico,
                new DateTime(2026, 7, 12),
                CategoriaGasto.Alimentacion,
                MetodoPago.PagoPersonal,
                pagador,
                1800m,
                "Cena con cliente",
                null);

            Assert.AreEqual(
                CategoriaGasto.Alimentacion,
                viatico.Categoria);

            Assert.AreEqual(
                MetodoPago.PagoPersonal,
                viatico.MetodoPago);

            Assert.AreSame(
                pagador,
                viatico.PagadoPor);

            Assert.AreEqual(
                1800m,
                viatico.Monto);

            Assert.AreEqual(
                "Cena con cliente",
                viatico.Descripcion);
        }

        [TestMethod]
        public void ModificarViatico_EnRendicion_RechazaOperacion()
        {
            Viaje viaje =
                CrearViaje();

            Viatico viatico =
                CrearViatico(
                    1,
                    1000m);

            viaje.AgregarViatico(
                viatico);

            viaje.EnviarARendicion(
                5,
                new DateTime(2026, 7, 20));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.ModificarViatico(
                        viatico,
                        viatico.Fecha,
                        viatico.Categoria,
                        viatico.MetodoPago,
                        viatico.PagadoPor,
                        2000m,
                        "Cambio no permitido",
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "no admite modificaciones");
        }

        [TestMethod]
        public void ModificarViatico_FechaFueraDelViaje_RechazaOperacion()
        {
            Viaje viaje =
                CrearViaje();

            Viatico viatico =
                CrearViatico(
                    1,
                    1000m);

            viaje.AgregarViatico(
                viatico);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.ModificarViatico(
                        viatico,
                        new DateTime(2026, 7, 15),
                        CategoriaGasto.Otros,
                        MetodoPago.EfectivoEmpresa,
                        null,
                        2000m,
                        "Cambio",
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "dentro del período");
        }

        [TestMethod]
        public void EnviarARendicion_ConAuditoria_RegistraUsuarioYFecha()
        {
            Viaje viaje =
                CrearViaje();

            DateTime fecha =
                new DateTime(
                    2026,
                    7,
                    20,
                    10,
                    0,
                    0);

            viaje.EnviarARendicion(
                7,
                fecha);

            Assert.AreEqual(
                EstadoViaje.EnRendicion,
                viaje.EstadoActual);

            Assert.AreEqual(
                7,
                viaje.IdUsuarioEnvioRendicion);

            Assert.AreEqual(
                fecha,
                viaje.FechaEnvioRendicion);
        }

        [TestMethod]
        public void EnviarARendicion_UsuarioInvalido_RechazaOperacion()
        {
            Viaje viaje =
                CrearViaje();

            Assert.ThrowsException<ReglaNegocioException>(
                () => viaje.EnviarARendicion(
                    0,
                    DateTime.Now));

            Assert.AreEqual(
                EstadoViaje.Abierto,
                viaje.EstadoActual);
        }

        [TestMethod]
        public void ExcluirViatico_ConAuditoria_RegistraDatos()
        {
            Viaje viaje =
                CrearViaje();

            Viatico viatico =
                CrearViatico(
                    1,
                    1000m);

            viaje.AgregarViatico(
                viatico);

            viaje.EnviarARendicion(
                5,
                new DateTime(2026, 7, 20));

            DateTime fechaExclusion =
                new DateTime(
                    2026,
                    7,
                    21,
                    9,
                    30,
                    0);

            viaje.ExcluirViatico(
                viatico,
                "Importe incorrecto",
                8,
                fechaExclusion);

            Assert.AreEqual(
                EstadoViatico.Excluido,
                viatico.Estado);

            Assert.AreEqual(
                "Importe incorrecto",
                viatico.MotivoExclusion);

            Assert.AreEqual(
                8,
                viatico.IdUsuarioExclusion);

            Assert.AreEqual(
                fechaExclusion,
                viatico.FechaExclusion);

            Assert.AreEqual(
                0m,
                viaje.TotalGastado);
        }

        [TestMethod]
        public void ExcluirViatico_SinMotivo_RechazaOperacion()
        {
            Viaje viaje =
                CrearViaje();

            Viatico viatico =
                CrearViatico(
                    1,
                    1000m);

            viaje.AgregarViatico(
                viatico);

            viaje.EnviarARendicion(
                5,
                DateTime.Now);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.ExcluirViatico(
                        viatico,
                        " ",
                        8,
                        DateTime.Now));

            StringAssert.Contains(
                excepcion.Message,
                "motivo");
        }

        [TestMethod]
        public void ReactivarViatico_ConAuditoria_RegistraDatos()
        {
            Viaje viaje =
                CrearViaje();

            Viatico viatico =
                CrearViatico(
                    1,
                    1000m);

            viaje.AgregarViatico(
                viatico);

            viaje.EnviarARendicion(
                5,
                DateTime.Now);

            viaje.ExcluirViatico(
                viatico,
                "Importe incorrecto",
                8,
                DateTime.Now);

            DateTime fechaReactivacion =
                new DateTime(
                    2026,
                    7,
                    22,
                    11,
                    0,
                    0);

            viaje.ReactivarViatico(
                viatico,
                9,
                fechaReactivacion);

            Assert.AreEqual(
                EstadoViatico.Vigente,
                viatico.Estado);

            Assert.AreEqual(
                9,
                viatico.IdUsuarioReactivacion);

            Assert.AreEqual(
                fechaReactivacion,
                viatico.FechaReactivacion);

            Assert.AreEqual(
                1000m,
                viaje.TotalGastado);
        }

        [TestMethod]
        public void AjustarMontoAnticipado_EnRendicion_ActualizaSaldo()
        {
            Viaje viaje =
                CrearViaje(
                    500m);

            viaje.AgregarViatico(
                CrearViatico(
                    1,
                    1500m));

            viaje.EnviarARendicion(
                5,
                DateTime.Now);

            viaje.AjustarMontoAnticipado(
                1200m);

            Assert.AreEqual(
                1200m,
                viaje.MontoAnticipado);

            Assert.AreEqual(
                300m,
                viaje.SaldoPendiente);
        }

        [TestMethod]
        public void AjustarMontoAnticipado_EnAbierto_RechazaOperacion()
        {
            Viaje viaje =
                CrearViaje();

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AjustarMontoAnticipado(
                        1000m));

            StringAssert.Contains(
                excepcion.Message,
                "EnRendicion");
        }

        [TestMethod]
        public void Aprobar_ConAuditoria_RegistraUsuarioYFecha()
        {
            Viaje viaje =
                CrearViaje();

            viaje.EnviarARendicion(
                5,
                DateTime.Now);

            DateTime fechaAprobacion =
                new DateTime(
                    2026,
                    7,
                    23,
                    12,
                    0,
                    0);

            viaje.Aprobar(
                10,
                fechaAprobacion);

            Assert.AreEqual(
                EstadoViaje.Aprobado,
                viaje.EstadoActual);

            Assert.AreEqual(
                10,
                viaje.IdUsuarioAprobador);

            Assert.AreEqual(
                fechaAprobacion,
                viaje.FechaAprobacion);
        }

        [TestMethod]
        public void Aprobar_SinViaticos_EsValido()
        {
            Viaje viaje =
                CrearViaje();

            viaje.EnviarARendicion(
                5,
                DateTime.Now);

            viaje.Aprobar(
                10,
                DateTime.Now);

            Assert.AreEqual(
                EstadoViaje.Aprobado,
                viaje.EstadoActual);

            Assert.AreEqual(
                0m,
                viaje.TotalGastado);
        }

        [TestMethod]
        public void Cancelar_ConAuditoria_RegistraMotivoUsuarioYFecha()
        {
            Viaje viaje =
                CrearViaje();

            viaje.EnviarARendicion(
                5,
                DateTime.Now);

            DateTime fechaCancelacion =
                new DateTime(
                    2026,
                    7,
                    24,
                    15,
                    0,
                    0);

            viaje.Cancelar(
                "Viaje suspendido por el cliente",
                11,
                fechaCancelacion);

            Assert.AreEqual(
                EstadoViaje.Cancelado,
                viaje.EstadoActual);

            Assert.AreEqual(
                "Viaje suspendido por el cliente",
                viaje.MotivoCancelacion);

            Assert.AreEqual(
                11,
                viaje.IdUsuarioCancelacion);

            Assert.AreEqual(
                fechaCancelacion,
                viaje.FechaCancelacion);
        }

        [TestMethod]
        public void Cancelar_SinMotivo_RechazaOperacion()
        {
            Viaje viaje =
                CrearViaje();

            viaje.EnviarARendicion(
                5,
                DateTime.Now);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.Cancelar(
                        " ",
                        11,
                        DateTime.Now));

            StringAssert.Contains(
                excepcion.Message,
                "motivo");
        }

        [TestMethod]
        public void Reconstruir_ConViaticosYAuditoria_RecuperaAgregadoCompleto()
        {
            DateTime fechaEnvio =
                new DateTime(
                    2026,
                    7,
                    20,
                    10,
                    0,
                    0);

            DateTime fechaAprobacion =
                new DateTime(
                    2026,
                    7,
                    21,
                    12,
                    0,
                    0);

            Viatico vigente =
                Viatico.Reconstruir(
                    10,
                    1,
                    new DateTime(2026, 7, 11),
                    CategoriaGasto.Alimentacion,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    1500m,
                    "Almuerzo",
                    null,
                    EstadoViatico.Vigente,
                    null,
                    null,
                    null,
                    null,
                    null);

            Viatico excluido =
                Viatico.Reconstruir(
                    11,
                    1,
                    new DateTime(2026, 7, 12),
                    CategoriaGasto.Peaje,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    500m,
                    "Peaje duplicado",
                    null,
                    EstadoViatico.Excluido,
                    "Comprobante duplicado",
                    8,
                    new DateTime(2026, 7, 20, 11, 0, 0),
                    null,
                    null);

            Viaje viaje =
                Viaje.Reconstruir(
                    1,
                    new DateTime(2026, 7, 10),
                    new DateTime(2026, 7, 12),
                    "Viaje aprobado",
                    TipoViaje.Desplazamiento,
                    1000m,
                    EstadoViaje.Aprobado,
                    new[]
                    {
                        CrearPersona()
                    },
                    new Visita[0],
                    new[]
                    {
                        vigente,
                        excluido
                    },
                    5,
                    fechaEnvio,
                    9,
                    fechaAprobacion,
                    null,
                    null,
                    null);

            Assert.AreEqual(
                EstadoViaje.Aprobado,
                viaje.EstadoActual);

            Assert.AreEqual(
                2,
                viaje.Viaticos.Count);

            Assert.AreEqual(
                1500m,
                viaje.TotalGastado);

            Assert.AreEqual(
                500m,
                viaje.SaldoPendiente);

            Assert.AreEqual(
                5,
                viaje.IdUsuarioEnvioRendicion);

            Assert.AreEqual(
                fechaEnvio,
                viaje.FechaEnvioRendicion);

            Assert.AreEqual(
                9,
                viaje.IdUsuarioAprobador);

            Assert.AreEqual(
                fechaAprobacion,
                viaje.FechaAprobacion);
        }

        [TestMethod]
        public void Reconstruir_ConViaticosDuplicados_RechazaOperacion()
        {
            Viatico primero =
                CrearViaticoReconstruido(
                    10,
                    1,
                    new DateTime(2026, 7, 11));

            Viatico segundo =
                CrearViaticoReconstruido(
                    10,
                    1,
                    new DateTime(2026, 7, 11));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => ReconstruirViaje(
                        EstadoViaje.Abierto,
                        new[]
                        {
                            primero,
                            segundo
                        }));

            StringAssert.Contains(
                excepcion.Message,
                "duplicados");
        }

        [TestMethod]
        public void Reconstruir_ConViaticoDeOtroViaje_RechazaOperacion()
        {
            Viatico viatico =
                CrearViaticoReconstruido(
                    10,
                    99,
                    new DateTime(2026, 7, 11));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => ReconstruirViaje(
                        EstadoViaje.Abierto,
                        new[]
                        {
                            viatico
                        }));

            StringAssert.Contains(
                excepcion.Message,
                "otro viaje");
        }

        [TestMethod]
        public void Reconstruir_ConViaticoFueraDelPeriodo_RechazaOperacion()
        {
            Viatico viatico =
                CrearViaticoReconstruido(
                    10,
                    1,
                    new DateTime(2026, 7, 20));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => ReconstruirViaje(
                        EstadoViaje.Abierto,
                        new[]
                        {
                            viatico
                        }));

            StringAssert.Contains(
                excepcion.Message,
                "dentro del período");
        }

        [TestMethod]
        public void Reconstruir_ConAuditoriaAprobacionIncompleta_RechazaOperacion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => Viaje.Reconstruir(
                        1,
                        new DateTime(2026, 7, 10),
                        new DateTime(2026, 7, 12),
                        "Viaje aprobado",
                        TipoViaje.Desplazamiento,
                        0m,
                        EstadoViaje.Aprobado,
                        new[]
                        {
                            CrearPersona()
                        },
                        new Visita[0],
                        new Viatico[0],
                        5,
                        DateTime.Now,
                        9,
                        null,
                        null,
                        null,
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "forma completa");
        }

        [TestMethod]
        public void Reconstruir_ViajeAbiertoConAuditoriaEnvio_RechazaOperacion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => Viaje.Reconstruir(
                        1,
                        new DateTime(2026, 7, 10),
                        new DateTime(2026, 7, 12),
                        "Viaje abierto",
                        TipoViaje.Desplazamiento,
                        0m,
                        EstadoViaje.Abierto,
                        new[]
                        {
                            CrearPersona()
                        },
                        new Visita[0],
                        new Viatico[0],
                        5,
                        DateTime.Now,
                        null,
                        null,
                        null,
                        null,
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "Abierto");
        }

        private static Viaje ReconstruirViaje(
            EstadoViaje estado,
            Viatico[] viaticos)
        {
            return Viaje.Reconstruir(
                1,
                new DateTime(2026, 7, 10),
                new DateTime(2026, 7, 12),
                "Viaje reconstruido",
                TipoViaje.Desplazamiento,
                0m,
                estado,
                new[]
                {
                    CrearPersona()
                },
                new Visita[0],
                viaticos,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        private static Viatico CrearViaticoReconstruido(
            int idViatico,
            int idViaje,
            DateTime fecha)
        {
            return Viatico.Reconstruir(
                idViatico,
                idViaje,
                fecha,
                CategoriaGasto.Otros,
                MetodoPago.EfectivoEmpresa,
                null,
                100m,
                "Gasto reconstruido",
                null,
                EstadoViatico.Vigente,
                null,
                null,
                null,
                null,
                null);
        }

        private static Viaje CrearViaje(
            decimal montoAnticipado = 0m)
        {
            return new Viaje(
                1,
                new DateTime(2026, 7, 10),
                new DateTime(2026, 7, 12),
                "Viaje comercial",
                TipoViaje.Desplazamiento,
                montoAnticipado);
        }

        private static Viatico CrearViatico(
            int idViatico,
            decimal monto)
        {
            return new Viatico(
                idViatico,
                new DateTime(2026, 7, 11),
                CategoriaGasto.Otros,
                MetodoPago.EfectivoEmpresa,
                null,
                monto,
                "Gasto de prueba",
                null);
        }

        private static Persona CrearPersona()
        {
            return new Persona(
                20,
                "Ana",
                "Administrativa",
                "ana@sigevip.local");
        }
    }
}