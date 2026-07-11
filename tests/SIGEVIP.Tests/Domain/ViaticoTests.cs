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
            var viatico = new Viatico(
                1,
                new DateTime(2026, 7, 10),
                1500m,
                "Combustible");

            Assert.AreEqual(1, viatico.IdViatico);
            Assert.AreEqual(new DateTime(2026, 7, 10), viatico.Fecha);
            Assert.AreEqual(1500m, viatico.Monto);
            Assert.AreEqual("Combustible", viatico.Descripcion);
            Assert.AreEqual(EstadoViatico.Vigente, viatico.Estado);
            Assert.IsTrue(viatico.EstaVigente);
        }

        [TestMethod]
        public void CrearViatico_ConMontoCero_LanzaReglaNegocioException()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viatico(
                        1,
                        new DateTime(2026, 7, 10),
                        0m,
                        "Monto inválido"));

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
                        -100m,
                        "Monto inválido"));

            StringAssert.Contains(
                excepcion.Message,
                "mayor que cero");
        }

        [TestMethod]
        public void CrearViatico_ConDescripcionNula_UtilizaCadenaVacia()
        {
            var viatico = new Viatico(
                1,
                new DateTime(2026, 7, 10),
                100m,
                null);

            Assert.AreEqual(string.Empty, viatico.Descripcion);
        }
    }
}
