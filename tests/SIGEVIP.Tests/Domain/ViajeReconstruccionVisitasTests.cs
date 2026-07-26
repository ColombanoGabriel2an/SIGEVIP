using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class ViajeReconstruccionVisitasTests
    {
        [TestMethod]
        public void Reconstruir_ConVisitaPersistida_CargaVisita()
        {
            Visita visita =
                CrearVisita(
                    10,
                    20,
                    new DateTime(
                        2026,
                        7,
                        11));

            Viaje viaje =
                CrearViajeReconstruido(
                    EstadoViaje.Abierto,
                    new[]
                    {
                        visita
                    });

            Assert.AreEqual(
                1,
                viaje.Visitas.Count);

            Assert.AreSame(
                visita,
                System.Linq.Enumerable.Single(
                    viaje.Visitas));
        }

        [TestMethod]
        public void Reconstruir_ConViajeEnRendicion_CargaVisitasSinModificarEstado()
        {
            Viaje viaje =
                CrearViajeReconstruido(
                    EstadoViaje.EnRendicion,
                    new[]
                    {
                        CrearVisita(
                            10,
                            20,
                            new DateTime(
                                2026,
                                7,
                                11))
                    });

            Assert.AreEqual(
                EstadoViaje.EnRendicion,
                viaje.EstadoActual);

            Assert.AreEqual(
                1,
                viaje.Visitas.Count);
        }

        [TestMethod]
        public void Reconstruir_ConVisitaDeOtroViaje_LanzaExcepcion()
        {
            Visita visita =
                CrearVisita(
                    10,
                    99,
                    new DateTime(
                        2026,
                        7,
                        11));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => CrearViajeReconstruido(
                        EstadoViaje.Abierto,
                        new[]
                        {
                            visita
                        }));

            StringAssert.Contains(
                excepcion.Message,
                "otro viaje");
        }

        [TestMethod]
        public void Reconstruir_ConVisitaFueraDelPeriodo_LanzaExcepcion()
        {
            Visita visita =
                CrearVisita(
                    10,
                    20,
                    new DateTime(
                        2026,
                        7,
                        20));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => CrearViajeReconstruido(
                        EstadoViaje.Abierto,
                        new[]
                        {
                            visita
                        }));

            StringAssert.Contains(
                excepcion.Message,
                "dentro del período");
        }

        [TestMethod]
        public void Reconstruir_ConVisitasDuplicadas_LanzaExcepcion()
        {
            Visita primera =
                CrearVisita(
                    10,
                    20,
                    new DateTime(
                        2026,
                        7,
                        11));

            Visita segunda =
                CrearVisita(
                    10,
                    20,
                    new DateTime(
                        2026,
                        7,
                        11));

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => CrearViajeReconstruido(
                        EstadoViaje.Abierto,
                        new[]
                        {
                            primera,
                            segunda
                        }));

            StringAssert.Contains(
                excepcion.Message,
                "duplicadas");
        }

        [TestMethod]
        public void Cancelar_ViajeReconstruidoConVisitas_LanzaExcepcion()
        {
            Viaje viaje =
                CrearViajeReconstruido(
                    EstadoViaje.Abierto,
                    new[]
                    {
                        CrearVisita(
                            10,
                            20,
                            new DateTime(
                                2026,
                                7,
                                11))
                    });

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.Cancelar());

            StringAssert.Contains(
                excepcion.Message,
                "visitas registradas");

            Assert.AreEqual(
                EstadoViaje.Abierto,
                viaje.EstadoActual);
        }

        [TestMethod]
        public void Reconstruir_ConOverloadAnterior_ConservaCompatibilidad()
        {
            Viaje viaje =
                Viaje.Reconstruir(
                    20,
                    new DateTime(
                        2026,
                        7,
                        10),
                    new DateTime(
                        2026,
                        7,
                        12),
                    "Viaje comercial",
                    TipoViaje.Desplazamiento,
                    0m,
                    EstadoViaje.Abierto,
                    new[]
                    {
                        CrearParticipante()
                    });

            Assert.AreEqual(
                0,
                viaje.Visitas.Count);

            Assert.AreEqual(
                1,
                viaje.Participantes.Count);
        }

        private static Viaje CrearViajeReconstruido(
            EstadoViaje estado,
            Visita[] visitas)
        {
            return Viaje.Reconstruir(
                20,
                new DateTime(
                    2026,
                    7,
                    10),
                new DateTime(
                    2026,
                    7,
                    12),
                "Viaje comercial",
                TipoViaje.Desplazamiento,
                0m,
                estado,
                new[]
                {
                    CrearParticipante()
                },
                visitas);
        }

        private static Visita CrearVisita(
            int idVisita,
            int idViaje,
            DateTime fecha)
        {
            Cliente cliente =
                new Cliente(
                    1,
                    "Empresa de prueba",
                    "30-12345678-9",
                    string.Empty,
                    string.Empty,
                    "Rosario",
                    "Santa Fe");

            return Visita.Reconstruir(
                idVisita,
                idViaje,
                fecha,
                "Reunión comercial",
                "Rosario",
                new[]
                {
                    cliente
                });
        }

        private static Persona CrearParticipante()
        {
            return new Persona(
                1,
                "Persona",
                "Prueba",
                "persona@correo.com");
        }
    }
}