using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class ComprobanteTests
    {
        [TestMethod]
        public void CrearComprobante_ConDatosValidos_CalculaTotal()
        {
            Comprobante comprobante =
                CrearComprobante(
                    1000m,
                    210m);

            Assert.AreEqual(
                1210m,
                comprobante.Total);

            Assert.AreEqual(
                "0001",
                comprobante.Sucursal);

            Assert.AreEqual(
                "00001234",
                comprobante.Numero);
        }

        [TestMethod]
        public void CrearComprobante_ConMontoGravadoNegativo_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => CrearComprobante(
                        -1m,
                        0m));

            StringAssert.Contains(
                excepcion.Message,
                "monto gravado");
        }

        [TestMethod]
        public void CrearComprobante_ConImpuestosNegativos_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => CrearComprobante(
                        100m,
                        -1m));

            StringAssert.Contains(
                excepcion.Message,
                "impuestos");
        }

        [TestMethod]
        public void CrearComprobante_ConSucursalDeLongitudInvalida_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Comprobante(
                        1,
                        TipoComprobante.FacturaB,
                        "30-12345678-9",
                        "Proveedor de prueba",
                        SituacionFiscal.ResponsableInscripto,
                        "001",
                        "00001234",
                        100m,
                        21m));

            StringAssert.Contains(
                excepcion.Message,
                "cuatro dígitos");
        }

        [TestMethod]
        public void CrearComprobante_ConSucursalNoNumerica_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Comprobante(
                        1,
                        TipoComprobante.FacturaB,
                        "30-12345678-9",
                        "Proveedor de prueba",
                        SituacionFiscal.ResponsableInscripto,
                        "00A1",
                        "00001234",
                        100m,
                        21m));

            StringAssert.Contains(
                excepcion.Message,
                "cuatro dígitos");
        }

        [TestMethod]
        public void CrearComprobante_ConNumeroDeLongitudInvalida_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Comprobante(
                        1,
                        TipoComprobante.FacturaB,
                        "30-12345678-9",
                        "Proveedor de prueba",
                        SituacionFiscal.ResponsableInscripto,
                        "0001",
                        "1234",
                        100m,
                        21m));

            StringAssert.Contains(
                excepcion.Message,
                "ocho dígitos");
        }

        [TestMethod]
        public void CrearComprobante_ConTipoNoDefinido_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Comprobante(
                        1,
                        (TipoComprobante)999,
                        "30-12345678-9",
                        "Proveedor de prueba",
                        SituacionFiscal.ResponsableInscripto,
                        "0001",
                        "00001234",
                        100m,
                        21m));

            StringAssert.Contains(
                excepcion.Message,
                "tipo de comprobante");
        }

        [TestMethod]
        public void CrearComprobante_ConSituacionFiscalNoDefinida_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Comprobante(
                        1,
                        TipoComprobante.FacturaB,
                        "30-12345678-9",
                        "Proveedor de prueba",
                        (SituacionFiscal)999,
                        "0001",
                        "00001234",
                        100m,
                        21m));

            StringAssert.Contains(
                excepcion.Message,
                "situación fiscal");
        }

        [TestMethod]
        public void Actualizar_ConDatosValidos_ReemplazaDatosYRecalculaTotal()
        {
            Comprobante comprobante =
                CrearComprobante(
                    1000m,
                    210m);

            comprobante.Actualizar(
                TipoComprobante.Ticket,
                "20-11111111-1",
                "Nuevo proveedor",
                SituacionFiscal.Monotributista,
                "0002",
                "00005678",
                500m,
                50m);

            Assert.AreEqual(
                TipoComprobante.Ticket,
                comprobante.Tipo);

            Assert.AreEqual(
                "Nuevo proveedor",
                comprobante.RazonSocialProveedor);

            Assert.AreEqual(
                "0002",
                comprobante.Sucursal);

            Assert.AreEqual(
                "00005678",
                comprobante.Numero);

            Assert.AreEqual(
                550m,
                comprobante.Total);
        }

        [TestMethod]
        public void CrearComprobante_ConIdentificadorNegativo_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Comprobante(
                        -1,
                        TipoComprobante.FacturaB,
                        "30-12345678-9",
                        "Proveedor",
                        SituacionFiscal.ResponsableInscripto,
                        "0001",
                        "00001234",
                        100m,
                        21m));

            StringAssert.Contains(
                excepcion.Message,
                "identificador");
        }

        [TestMethod]
        public void CrearComprobante_SinCuitProveedor_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Comprobante(
                        0,
                        TipoComprobante.FacturaB,
                        " ",
                        "Proveedor",
                        SituacionFiscal.ResponsableInscripto,
                        "0001",
                        "00001234",
                        100m,
                        21m));

            StringAssert.Contains(
                excepcion.Message,
                "CUIT");
        }

        [TestMethod]
        public void CrearComprobante_SinRazonSocial_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Comprobante(
                        0,
                        TipoComprobante.FacturaB,
                        "30-12345678-9",
                        " ",
                        SituacionFiscal.ResponsableInscripto,
                        "0001",
                        "00001234",
                        100m,
                        21m));

            StringAssert.Contains(
                excepcion.Message,
                "razón social");
        }

        [TestMethod]
        public void Actualizar_ConDatosInvalidos_ConservaDatosAnteriores()
        {
            Comprobante comprobante =
                CrearComprobante(
                    1000m,
                    210m);

            Assert.ThrowsException<ReglaNegocioException>(
                () => comprobante.Actualizar(
                    TipoComprobante.Ticket,
                    "20-11111111-1",
                    "Proveedor inválido",
                    SituacionFiscal.Monotributista,
                    "001",
                    "00005678",
                    500m,
                    50m));

            Assert.AreEqual(
                TipoComprobante.FacturaB,
                comprobante.Tipo);

            Assert.AreEqual(
                "Proveedor de prueba",
                comprobante.RazonSocialProveedor);

            Assert.AreEqual(
                1210m,
                comprobante.Total);
        }

        private static Comprobante CrearComprobante(
            decimal montoGravado,
            decimal montoImpuestos)
        {
            return new Comprobante(
                1,
                TipoComprobante.FacturaB,
                "30-12345678-9",
                "Proveedor de prueba",
                SituacionFiscal.ResponsableInscripto,
                "0001",
                "00001234",
                montoGravado,
                montoImpuestos);
        }
    }
}