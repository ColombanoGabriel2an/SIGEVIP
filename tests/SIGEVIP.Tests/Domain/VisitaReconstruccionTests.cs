using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class VisitaReconstruccionTests
    {
        [TestMethod]
        public void Reconstruir_ConDatosValidos_ConservaDatos()
        {
            Cliente cliente =
                CrearCliente(
                    1);

            Visita visita =
                Visita.Reconstruir(
                    10,
                    20,
                    new DateTime(
                        2026,
                        7,
                        11,
                        15,
                        30,
                        0),
                    "Reunión comercial",
                    "Rosario",
                    new[]
                    {
                        cliente
                    });

            Assert.AreEqual(
                10,
                visita.IdVisita);

            Assert.AreEqual(
                20,
                visita.IdViaje);

            Assert.AreEqual(
                new DateTime(
                    2026,
                    7,
                    11),
                visita.Fecha);

            Assert.AreEqual(
                "Reunión comercial",
                visita.Observacion);

            Assert.AreEqual(
                "Rosario",
                visita.LocalidadEncuentro);

            Assert.AreEqual(
                1,
                visita.Clientes.Count);
        }

        [TestMethod]
        public void Reconstruir_ConIdVisitaInvalido_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => Visita.Reconstruir(
                        0,
                        20,
                        new DateTime(
                            2026,
                            7,
                            11),
                        "Reunión",
                        "Rosario",
                        new[]
                        {
                            CrearCliente(1)
                        }));

            StringAssert.Contains(
                excepcion.Message,
                "identificador persistido de la visita");
        }

        [TestMethod]
        public void Reconstruir_ConIdViajeInvalido_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => Visita.Reconstruir(
                        10,
                        0,
                        new DateTime(
                            2026,
                            7,
                            11),
                        "Reunión",
                        "Rosario",
                        new[]
                        {
                            CrearCliente(1)
                        }));

            StringAssert.Contains(
                excepcion.Message,
                "identificador persistido del viaje");
        }

        [TestMethod]
        public void Reconstruir_SinClientes_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => Visita.Reconstruir(
                        10,
                        20,
                        new DateTime(
                            2026,
                            7,
                            11),
                        "Reunión",
                        "Rosario",
                        new Cliente[0]));

            StringAssert.Contains(
                excepcion.Message,
                "al menos un cliente");
        }

        [TestMethod]
        public void Reconstruir_ConClientesNulos_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => Visita.Reconstruir(
                        10,
                        20,
                        new DateTime(
                            2026,
                            7,
                            11),
                        "Reunión",
                        "Rosario",
                        null));

            StringAssert.Contains(
                excepcion.Message,
                "clientes de la visita");
        }

        [TestMethod]
        public void Reconstruir_ConClienteInactivo_ConservaHistorial()
        {
            Cliente cliente =
                CrearCliente(
                    1);

            cliente.Desactivar();

            Visita visita =
                Visita.Reconstruir(
                    10,
                    20,
                    new DateTime(
                        2026,
                        7,
                        11),
                    "Reunión",
                    "Rosario",
                    new[]
                    {
                        cliente
                    });

            Assert.AreEqual(
                1,
                visita.Clientes.Count);

            Assert.IsFalse(
                cliente.Activo);
        }

        [TestMethod]
        public void Reconstruir_ConClientesDuplicados_LanzaExcepcion()
        {
            Cliente primero =
                CrearCliente(
                    1);

            Cliente segundo =
                CrearCliente(
                    1);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => Visita.Reconstruir(
                        10,
                        20,
                        new DateTime(
                            2026,
                            7,
                            11),
                        "Reunión",
                        "Rosario",
                        new[]
                        {
                            primero,
                            segundo
                        }));

            StringAssert.Contains(
                excepcion.Message,
                "duplicados");
        }

        private static Cliente CrearCliente(
            int idCliente)
        {
            return new Cliente(
                idCliente,
                "Empresa " +
                    idCliente,
                "30-1234567" +
                    idCliente +
                    "-9",
                string.Empty,
                string.Empty,
                "Rosario",
                "Santa Fe");
        }
    }
}