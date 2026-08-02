using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class VisitaTests
    {
        [TestMethod]
        public void CrearVisita_ConObservacionVacia_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Visita(
                        1,
                        new DateTime(2026, 7, 11),
                        " ",
                        "Rosario"));

            StringAssert.Contains(
                excepcion.Message,
                "observación");
        }

        [TestMethod]
        public void CrearVisita_ConLocalidadEncuentroVacia_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Visita(
                        1,
                        new DateTime(2026, 7, 11),
                        "Reunión comercial",
                        " "));

            StringAssert.Contains(
                excepcion.Message,
                "localidad");
        }

        [TestMethod]
        public void AgregarCliente_ConClienteNulo_LanzaExcepcion()
        {
            Visita visita = CrearVisita();

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => visita.AgregarCliente(null));

            StringAssert.Contains(
                excepcion.Message,
                "cliente válido");
        }

        [TestMethod]
        public void AgregarCliente_ConClienteValido_AsociaCliente()
        {
            Visita visita = CrearVisita();
            Cliente cliente = CrearCliente(
                1,
                "30-11111111-1");

            visita.AgregarCliente(cliente);

            Assert.AreEqual(1, visita.Clientes.Count);
            Assert.IsTrue(visita.TieneClientes);
        }

        [TestMethod]
        public void AgregarCliente_ConVariosClientes_AsociaTodos()
        {
            Visita visita = CrearVisita();

            visita.AgregarCliente(
                CrearCliente(1, "30-11111111-1"));

            visita.AgregarCliente(
                CrearCliente(2, "30-22222222-2"));

            Assert.AreEqual(2, visita.Clientes.Count);
        }

        [TestMethod]
        public void AgregarCliente_MismaReferenciaDosVeces_LanzaExcepcion()
        {
            Visita visita = CrearVisita();
            Cliente cliente = CrearCliente(
                0,
                "30-11111111-1");

            visita.AgregarCliente(cliente);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => visita.AgregarCliente(cliente));

            StringAssert.Contains(
                excepcion.Message,
                "ya se encuentra asociado");
        }

        [TestMethod]
        public void AgregarCliente_MismoIdPersistido_LanzaExcepcion()
        {
            Visita visita = CrearVisita();

            Cliente primero = CrearCliente(
                15,
                "30-11111111-1");

            Cliente segundo = CrearCliente(
                15,
                "30-22222222-2");

            visita.AgregarCliente(primero);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => visita.AgregarCliente(segundo));

            StringAssert.Contains(
                excepcion.Message,
                "ya se encuentra asociado");
        }

        [TestMethod]
        public void AgregarCliente_MismoCuitNormalizado_LanzaExcepcion()
        {
            Visita visita = CrearVisita();

            Cliente primero = CrearCliente(
                0,
                "30-11111111-1");

            Cliente segundo = CrearCliente(
                0,
                "30111111111");

            visita.AgregarCliente(primero);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => visita.AgregarCliente(segundo));

            StringAssert.Contains(
                excepcion.Message,
                "ya se encuentra asociado");
        }

        [TestMethod]
        public void Modificar_DatosYClientesValidos_ActualizaVisita()
        {
            Visita visita = CrearVisita();

            Cliente primero =
                CrearCliente(
                    1,
                    "30-11111111-1");

            Cliente segundo =
                CrearCliente(
                    2,
                    "30-22222222-2");

            visita.AgregarCliente(
                primero);

            visita.Modificar(
                new DateTime(2026, 7, 12),
                "Seguimiento actualizado",
                "Funes",
                new[]
                {
                    segundo
                });

            Assert.AreEqual(
                new DateTime(2026, 7, 12),
                visita.Fecha);

            Assert.AreEqual(
                "Seguimiento actualizado",
                visita.Observacion);

            Assert.AreEqual(
                "Funes",
                visita.LocalidadEncuentro);

            Assert.AreEqual(
                1,
                visita.Clientes.Count);

            Assert.AreEqual(
                2,
                visita.Clientes.Single().IdCliente);
        }

        [TestMethod]
        public void Modificar_SinClientes_RechazaOperacion()
        {
            Visita visita = CrearVisita();

            Assert.ThrowsException<ReglaNegocioException>(
                () => visita.Modificar(
                    new DateTime(2026, 7, 12),
                    "Seguimiento actualizado",
                    "Funes",
                    new Cliente[0]));
        }

        [TestMethod]
        public void Clientes_NoPermiteModificarColeccionDesdeElExterior()
        {
            Visita visita = CrearVisita();

            Assert.IsFalse(
                visita.Clientes is List<Cliente>);
        }

        private static Visita CrearVisita()
        {
            return new Visita(
                1,
                new DateTime(2026, 7, 11),
                "Reunión comercial",
                "Rosario");
        }

        private static Cliente CrearCliente(
            int idCliente,
            string cuit)
        {
            return new Cliente(
                idCliente,
                "Empresa " + cuit,
                cuit,
                string.Empty,
                string.Empty,
                "Rosario",
                "Santa Fe");
        }
    }
}
