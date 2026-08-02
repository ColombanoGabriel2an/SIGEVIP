using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class ViajeVisitaTests
    {
        [TestMethod]
        public void AgregarVisita_SinClientes_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();
            Visita visita = CrearVisita(
                1,
                new DateTime(2026, 7, 11));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarVisita(visita));

            StringAssert.Contains(
                excepcion.Message,
                "al menos un cliente");
        }

        [TestMethod]
        public void AgregarVisita_EnViajeAbierto_AgregaVisitaValida()
        {
            Viaje viaje = CrearViaje();
            Visita visita = CrearVisitaConCliente(
                1,
                new DateTime(2026, 7, 11));

            viaje.AgregarVisita(visita);

            Assert.AreEqual(
                1,
                viaje.Visitas.Count);

            Assert.AreEqual(
                viaje.IdViaje,
                visita.IdViaje);
        }

        [TestMethod]
        public void AgregarVisita_EnFechaInicio_AgregaVisita()
        {
            Viaje viaje = CrearViaje();
            Visita visita = CrearVisitaConCliente(
                1,
                new DateTime(2026, 7, 10));

            viaje.AgregarVisita(visita);

            Assert.AreEqual(
                1,
                viaje.Visitas.Count);
        }

        [TestMethod]
        public void AgregarVisita_EnFechaFin_AgregaVisita()
        {
            Viaje viaje = CrearViaje();
            Visita visita = CrearVisitaConCliente(
                1,
                new DateTime(2026, 7, 12));

            viaje.AgregarVisita(visita);

            Assert.AreEqual(
                1,
                viaje.Visitas.Count);
        }

        [TestMethod]
        public void AgregarVisita_ConFechaAnteriorAlViaje_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();
            Visita visita = CrearVisitaConCliente(
                1,
                new DateTime(2026, 7, 9));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarVisita(visita));

            StringAssert.Contains(
                excepcion.Message,
                "dentro del período");
        }

        [TestMethod]
        public void AgregarVisita_ConFechaPosteriorAlViaje_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();
            Visita visita = CrearVisitaConCliente(
                1,
                new DateTime(2026, 7, 13));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarVisita(visita));

            StringAssert.Contains(
                excepcion.Message,
                "dentro del período");
        }

        [TestMethod]
        public void AgregarVisita_Nula_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarVisita(null));

            StringAssert.Contains(
                excepcion.Message,
                "visita válida");
        }

        [TestMethod]
        public void AgregarVisita_EnRendicion_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();
            viaje.EnviarARendicion();

            Visita visita = CrearVisitaConCliente(
                1,
                new DateTime(2026, 7, 11));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarVisita(visita));

            StringAssert.Contains(
                excepcion.Message,
                "no admite modificaciones");
        }

        [TestMethod]
        public void AgregarVisita_EnViajeAprobado_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();
            viaje.EnviarARendicion();
            viaje.Aprobar();

            Visita visita = CrearVisitaConCliente(
                1,
                new DateTime(2026, 7, 11));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarVisita(visita));

            StringAssert.Contains(
                excepcion.Message,
                "Aprobado");
        }

        [TestMethod]
        public void AgregarVisita_EnViajeCancelado_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();
            viaje.Cancelar();

            Visita visita = CrearVisitaConCliente(
                1,
                new DateTime(2026, 7, 11));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarVisita(visita));

            StringAssert.Contains(
                excepcion.Message,
                "Cancelado");
        }

        [TestMethod]
        public void AgregarVisita_MismaReferenciaDosVeces_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();
            Visita visita = CrearVisitaConCliente(
                0,
                new DateTime(2026, 7, 11));

            viaje.AgregarVisita(visita);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarVisita(visita));

            StringAssert.Contains(
                excepcion.Message,
                "ya fue agregada");
        }

        [TestMethod]
        public void AgregarVisita_MismoIdPersistido_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();

            Visita primera = CrearVisitaConCliente(
                15,
                new DateTime(2026, 7, 11));

            Visita segunda = CrearVisitaConCliente(
                15,
                new DateTime(2026, 7, 11));

            viaje.AgregarVisita(primera);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarVisita(segunda));

            StringAssert.Contains(
                excepcion.Message,
                "ya fue agregada");
        }

        [TestMethod]
        public void AgregarVisita_AsociadaAOtroViaje_LanzaExcepcion()
        {
            Viaje primerViaje = CrearViaje(1);
            Viaje segundoViaje = CrearViaje(2);

            Visita visita = CrearVisitaConCliente(
                1,
                new DateTime(2026, 7, 11));

            primerViaje.AgregarVisita(visita);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => segundoViaje.AgregarVisita(visita));

            StringAssert.Contains(
                excepcion.Message,
                "otro viaje");
        }

        [TestMethod]
        public void ModificarVisita_EnViajeAbierto_ActualizaDatosYClientes()
        {
            Viaje viaje = CrearViaje();

            Visita visita =
                CrearVisitaConCliente(
                    15,
                    new DateTime(2026, 7, 11));

            viaje.AgregarVisita(
                visita);

            Cliente nuevoCliente =
                new Cliente(
                    200,
                    "Empresa modificada",
                    "30-20000000-1",
                    string.Empty,
                    string.Empty,
                    "Funes",
                    "Santa Fe");

            viaje.ModificarVisita(
                visita,
                new DateTime(2026, 7, 12),
                "Visita modificada",
                "Funes",
                new[]
                {
                    nuevoCliente
                });

            Assert.AreEqual(
                new DateTime(2026, 7, 12),
                visita.Fecha);

            Assert.AreEqual(
                "Visita modificada",
                visita.Observacion);

            Assert.AreEqual(
                200,
                visita.Clientes.Single().IdCliente);
        }

        [TestMethod]
        public void ModificarVisita_EnRendicion_RechazaOperacion()
        {
            Viaje viaje = CrearViaje();

            Visita visita =
                CrearVisitaConCliente(
                    15,
                    new DateTime(2026, 7, 11));

            viaje.AgregarVisita(
                visita);

            viaje.EnviarARendicion();

            Assert.ThrowsException<ReglaNegocioException>(
                () => viaje.ModificarVisita(
                    visita,
                    visita.Fecha,
                    visita.Observacion,
                    visita.LocalidadEncuentro,
                    visita.Clientes));
        }

        [TestMethod]
        public void ModificarVisita_DeOtroViaje_RechazaOperacion()
        {
            Viaje viaje = CrearViaje();

            Visita visitaAjena =
                CrearVisitaConCliente(
                    99,
                    new DateTime(2026, 7, 11));

            Assert.ThrowsException<ReglaNegocioException>(
                () => viaje.ModificarVisita(
                    visitaAjena,
                    visitaAjena.Fecha,
                    visitaAjena.Observacion,
                    visitaAjena.LocalidadEncuentro,
                    visitaAjena.Clientes));
        }

        [TestMethod]
        public void ModificarVisita_ConFechaFueraDelViaje_RechazaOperacion()
        {
            Viaje viaje = CrearViaje();

            Visita visita =
                CrearVisitaConCliente(
                    15,
                    new DateTime(2026, 7, 11));

            viaje.AgregarVisita(
                visita);

            Assert.ThrowsException<ReglaNegocioException>(
                () => viaje.ModificarVisita(
                    visita,
                    new DateTime(2026, 7, 20),
                    visita.Observacion,
                    visita.LocalidadEncuentro,
                    visita.Clientes));
        }

        [TestMethod]
        public void Visitas_NoPermiteModificarColeccionDesdeElExterior()
        {
            Viaje viaje = CrearViaje();

            Assert.IsFalse(
                viaje.Visitas is List<Visita>);
        }

        [TestMethod]
        public void Cancelar_DesdeAbiertoConVisitas_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();

            viaje.AgregarVisita(
                CrearVisitaConCliente(
                    1,
                    new DateTime(2026, 7, 11)));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.Cancelar());

            StringAssert.Contains(
                excepcion.Message,
                "visitas registradas");
        }

        [TestMethod]
        public void Cancelar_DesdeAbiertoConVisitas_ConservaEstadoAbierto()
        {
            Viaje viaje = CrearViaje();

            viaje.AgregarVisita(
                CrearVisitaConCliente(
                    1,
                    new DateTime(2026, 7, 11)));

            Assert.ThrowsException<ReglaNegocioException>(
                () => viaje.Cancelar());

            Assert.AreEqual(
                EstadoViaje.Abierto,
                viaje.EstadoActual);
        }

        [TestMethod]
        public void Cancelar_DesdeEnRendicionConVisitas_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();

            viaje.AgregarVisita(
                CrearVisitaConCliente(
                    1,
                    new DateTime(2026, 7, 11)));

            viaje.EnviarARendicion();

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.Cancelar());

            StringAssert.Contains(
                excepcion.Message,
                "visitas registradas");
        }

        [TestMethod]
        public void Cancelar_DesdeEnRendicionConVisitas_ConservaEstadoEnRendicion()
        {
            Viaje viaje = CrearViaje();

            viaje.AgregarVisita(
                CrearVisitaConCliente(
                    1,
                    new DateTime(2026, 7, 11)));

            viaje.EnviarARendicion();

            Assert.ThrowsException<ReglaNegocioException>(
                () => viaje.Cancelar());

            Assert.AreEqual(
                EstadoViaje.EnRendicion,
                viaje.EstadoActual);
        }

        private static Viaje CrearViaje(
            int idViaje = 1)
        {
            return new Viaje(
                idViaje,
                new DateTime(2026, 7, 10),
                new DateTime(2026, 7, 12),
                "Viaje comercial",
                TipoViaje.Desplazamiento,
                0m);
        }

        private static Visita CrearVisita(
            int idVisita,
            DateTime fecha)
        {
            return new Visita(
                idVisita,
                fecha,
                "Reunión comercial",
                "Rosario");
        }

        private static Visita CrearVisitaConCliente(
            int idVisita,
            DateTime fecha)
        {
            Visita visita =
                CrearVisita(idVisita, fecha);

            visita.AgregarCliente(
                new Cliente(
                    idVisita + 100,
                    "Empresa de prueba " + idVisita,
                    "30-1234567" + idVisita + "-9",
                    string.Empty,
                    string.Empty,
                    "Rosario",
                    "Santa Fe"));

            return visita;
        }
    }
}
