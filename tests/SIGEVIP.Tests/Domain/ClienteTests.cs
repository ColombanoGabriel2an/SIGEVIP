using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class ClienteTests
    {
        [TestMethod]
        public void CrearCliente_ConDatosValidos_NaceActivo()
        {
            Cliente cliente =
                CrearCliente();

            Assert.IsTrue(
                cliente.Activo);
        }

        [TestMethod]
        public void CrearCliente_ConRazonSocialVacia_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Cliente(
                        1,
                        " ",
                        "30-12345678-9",
                        string.Empty,
                        string.Empty,
                        "Rosario",
                        "Santa Fe"));

            StringAssert.Contains(
                excepcion.Message,
                "razón social");
        }

        [TestMethod]
        public void CrearCliente_ConCuitVacio_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Cliente(
                        1,
                        "Empresa de prueba",
                        " ",
                        string.Empty,
                        string.Empty,
                        "Rosario",
                        "Santa Fe"));

            StringAssert.Contains(
                excepcion.Message,
                "CUIT");
        }

        [TestMethod]
        public void DesactivarCliente_CambiaEstadoAInactivo()
        {
            Cliente cliente =
                CrearCliente();

            cliente.Desactivar();

            Assert.IsFalse(
                cliente.Activo);
        }

        [TestMethod]
        public void ActivarCliente_DespuesDeDesactivarlo_CambiaEstadoAActivo()
        {
            Cliente cliente =
                CrearCliente();

            cliente.Desactivar();
            cliente.Activar();

            Assert.IsTrue(
                cliente.Activo);
        }

        [TestMethod]
        public void CrearCliente_NormalizaEspaciosYGuionesDelCuit()
        {
            Cliente cliente =
                new Cliente(
                    1,
                    "Empresa de prueba",
                    " 30-12345678-9 ",
                    "empresa@prueba.com",
                    "3415550000",
                    "Rosario",
                    "Santa Fe");

            Assert.AreEqual(
                "30123456789",
                cliente.Cuit);
        }

        [TestMethod]
        public void ActualizarDatos_ConDatosValidos_ModificaCampos()
        {
            Cliente cliente =
                CrearCliente();

            cliente.ActualizarDatos(
                "Empresa actualizada",
                "30-99999999-1",
                "actualizada@prueba.com",
                "3415551111",
                "Funes",
                "Santa Fe");

            Assert.AreEqual(
                "Empresa actualizada",
                cliente.RazonSocial);

            Assert.AreEqual(
                "30999999991",
                cliente.Cuit);

            Assert.AreEqual(
                "Funes",
                cliente.Localidad);
        }

        [TestMethod]
        public void ActualizarDatos_NoModificaIdNiEstado()
        {
            Cliente cliente =
                CrearCliente();

            cliente.Desactivar();

            cliente.ActualizarDatos(
                "Empresa actualizada",
                "30-99999999-1",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);

            Assert.AreEqual(
                1,
                cliente.IdCliente);

            Assert.IsFalse(
                cliente.Activo);
        }

        [TestMethod]
        public void Reconstruir_ConEstadoInactivo_ConservaEstadoPersistido()
        {
            Cliente cliente =
                Cliente.Reconstruir(
                    8,
                    "Empresa persistida",
                    "30-11111111-1",
                    string.Empty,
                    string.Empty,
                    "Rosario",
                    "Santa Fe",
                    false);

            Assert.AreEqual(
                8,
                cliente.IdCliente);

            Assert.IsFalse(
                cliente.Activo);
        }

        private static Cliente CrearCliente()
        {
            return new Cliente(
                1,
                "Empresa de prueba",
                "30-12345678-9",
                "empresa@prueba.com",
                "3415550000",
                "Rosario",
                "Santa Fe");
        }
    }
}
