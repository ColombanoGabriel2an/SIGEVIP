using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class ViaticoTests
    {
        [TestMethod]
        public void CrearViatico_ConDatosValidos_NaceVigente()
        {
            Persona pagador =
                CrearPersona();

            Comprobante comprobante =
                CrearComprobante();

            var viatico =
                new Viatico(
                    1,
                    new DateTime(2026, 7, 10),
                    CategoriaGasto.Alimentacion,
                    MetodoPago.PagoPersonal,
                    pagador,
                    1500m,
                    "Almuerzo durante el viaje",
                    comprobante);

            Assert.AreEqual(
                1,
                viatico.IdViatico);

            Assert.AreEqual(
                new DateTime(2026, 7, 10),
                viatico.Fecha);

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
                1500m,
                viatico.Monto);

            Assert.AreEqual(
                "Almuerzo durante el viaje",
                viatico.Descripcion);

            Assert.AreSame(
                comprobante,
                viatico.Comprobante);

            Assert.AreEqual(
                EstadoViatico.Vigente,
                viatico.Estado);

            Assert.IsTrue(
                viatico.EstaVigente);

            Assert.IsTrue(
                viatico.TieneComprobante);
        }

        [TestMethod]
        public void CrearViatico_ConstructorAnterior_ConservaCompatibilidad()
        {
            var viatico =
                new Viatico(
                    1,
                    new DateTime(2026, 7, 10),
                    1500m,
                    "Gasto histórico");

            Assert.AreEqual(
                CategoriaGasto.Otros,
                viatico.Categoria);

            Assert.AreEqual(
                MetodoPago.EfectivoEmpresa,
                viatico.MetodoPago);

            Assert.IsNull(
                viatico.PagadoPor);

            Assert.IsNull(
                viatico.Comprobante);
        }

        [TestMethod]
        public void CrearViatico_ConMontoCero_LanzaReglaNegocioException()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viatico(
                        1,
                        new DateTime(2026, 7, 10),
                        CategoriaGasto.Peaje,
                        MetodoPago.EfectivoEmpresa,
                        null,
                        0m,
                        "Monto inválido",
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "mayor que cero");
        }

        [TestMethod]
        public void CrearViatico_ConMontoNegativo_LanzaReglaNegocioException()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viatico(
                        1,
                        new DateTime(2026, 7, 10),
                        CategoriaGasto.Peaje,
                        MetodoPago.EfectivoEmpresa,
                        null,
                        -100m,
                        "Monto inválido",
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "mayor que cero");
        }

        [TestMethod]
        public void CrearViatico_SinComprobanteNiDescripcion_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viatico(
                        1,
                        new DateTime(2026, 7, 10),
                        CategoriaGasto.Otros,
                        MetodoPago.EfectivoEmpresa,
                        null,
                        100m,
                        " ",
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "justificar");
        }

        [TestMethod]
        public void CrearViatico_ConComprobantePermiteDescripcionVacia()
        {
            var viatico =
                new Viatico(
                    1,
                    new DateTime(2026, 7, 10),
                    CategoriaGasto.Alojamiento,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    1000m,
                    null,
                    CrearComprobante());

            Assert.AreEqual(
                string.Empty,
                viatico.Descripcion);

            Assert.IsTrue(
                viatico.TieneComprobante);
        }

        [TestMethod]
        public void CrearViatico_PagoPersonalSinPagador_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viatico(
                        1,
                        new DateTime(2026, 7, 10),
                        CategoriaGasto.Alimentacion,
                        MetodoPago.PagoPersonal,
                        null,
                        100m,
                        "Gasto personal",
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "persona pagadora");
        }

        [TestMethod]
        public void CrearViatico_TarjetaCorporativaSinPagador_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viatico(
                        1,
                        new DateTime(2026, 7, 10),
                        CategoriaGasto.Combustible,
                        MetodoPago.TarjetaCorporativa,
                        null,
                        100m,
                        "Carga de combustible",
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "persona pagadora");
        }

        [TestMethod]
        public void CrearViatico_EfectivoEmpresaConPagador_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viatico(
                        1,
                        new DateTime(2026, 7, 10),
                        CategoriaGasto.Peaje,
                        MetodoPago.EfectivoEmpresa,
                        CrearPersona(),
                        100m,
                        "Peaje",
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "no admite");
        }

        [TestMethod]
        public void CrearViatico_ConCategoriaNoDefinida_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viatico(
                        1,
                        new DateTime(2026, 7, 10),
                        (CategoriaGasto)999,
                        MetodoPago.EfectivoEmpresa,
                        null,
                        100m,
                        "Categoría inválida",
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "categoría");
        }

        [TestMethod]
        public void CrearViatico_ConMetodoNoDefinido_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viatico(
                        1,
                        new DateTime(2026, 7, 10),
                        CategoriaGasto.Otros,
                        (MetodoPago)999,
                        null,
                        100m,
                        "Método inválido",
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "método de pago");
        }

        [TestMethod]
        public void CrearViatico_NormalizaDescripcion()
        {
            var viatico =
                new Viatico(
                    1,
                    new DateTime(2026, 7, 10),
                    CategoriaGasto.Otros,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    100m,
                    "  Gasto justificado  ",
                    null);

            Assert.AreEqual(
                "Gasto justificado",
                viatico.Descripcion);
        }

        [TestMethod]
        public void ReconstruirViatico_Excluido_RecuperaAuditoriaYViaje()
        {
            DateTime fechaExclusion =
                new DateTime(
                    2026,
                    7,
                    20,
                    10,
                    30,
                    0);

            Viatico viatico =
                Viatico.Reconstruir(
                    15,
                    8,
                    new DateTime(2026, 7, 11),
                    CategoriaGasto.Alojamiento,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    2000m,
                    "Hotel",
                    CrearComprobante(),
                    EstadoViatico.Excluido,
                    "Comprobante incorrecto",
                    5,
                    fechaExclusion,
                    null,
                    null);

            Assert.AreEqual(
                15,
                viatico.IdViatico);

            Assert.AreEqual(
                8,
                viatico.IdViaje);

            Assert.AreEqual(
                EstadoViatico.Excluido,
                viatico.Estado);

            Assert.AreEqual(
                "Comprobante incorrecto",
                viatico.MotivoExclusion);

            Assert.AreEqual(
                5,
                viatico.IdUsuarioExclusion);

            Assert.AreEqual(
                fechaExclusion,
                viatico.FechaExclusion);
        }

        [TestMethod]
        public void ReconstruirViatico_ExcluidoSinMotivo_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => Viatico.Reconstruir(
                        15,
                        8,
                        new DateTime(2026, 7, 11),
                        CategoriaGasto.Alojamiento,
                        MetodoPago.EfectivoEmpresa,
                        null,
                        2000m,
                        "Hotel",
                        CrearComprobante(),
                        EstadoViatico.Excluido,
                        null,
                        5,
                        DateTime.Now,
                        null,
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "motivo");
        }

        [TestMethod]
        public void ReconstruirViatico_ConAuditoriaReactivacionIncompleta_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => Viatico.Reconstruir(
                        15,
                        8,
                        new DateTime(2026, 7, 11),
                        CategoriaGasto.Alojamiento,
                        MetodoPago.EfectivoEmpresa,
                        null,
                        2000m,
                        "Hotel",
                        CrearComprobante(),
                        EstadoViatico.Vigente,
                        null,
                        null,
                        null,
                        6,
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "forma completa");
        }

        private static Persona CrearPersona()
        {
            return new Persona(
                10,
                "Ana",
                "Administrativa",
                "ana@sigevip.local");
        }

        private static Comprobante
            CrearComprobante()
        {
            return new Comprobante(
                20,
                TipoComprobante.FacturaB,
                "30-12345678-9",
                "Proveedor de prueba",
                SituacionFiscal.ResponsableInscripto,
                "0001",
                "00001234",
                1000m,
                210m);
        }
    }
}
