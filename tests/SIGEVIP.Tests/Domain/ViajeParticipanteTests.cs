using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class ViajeParticipanteTests
    {
        [TestMethod]
        public void AgregarParticipante_Valido_LoIncorpora()
        {
            Viaje viaje = CrearViaje();

            viaje.AgregarParticipante(
                CrearPersona(1));

            Assert.AreEqual(
                1,
                viaje.Participantes.Count);
        }

        [TestMethod]
        public void AgregarParticipante_Varios_LosIncorpora()
        {
            Viaje viaje = CrearViaje();

            viaje.AgregarParticipante(
                CrearPersona(1));

            viaje.AgregarParticipante(
                CrearPersona(2));

            Assert.AreEqual(
                2,
                viaje.Participantes.Count);
        }

        [TestMethod]
        public void AgregarParticipante_Nulo_RechazaOperacion()
        {
            Viaje viaje = CrearViaje();

            Assert.ThrowsException<ReglaNegocioException>(
                () => viaje.AgregarParticipante(
                    null));
        }

        [TestMethod]
        public void AgregarParticipante_MismoId_RechazaDuplicado()
        {
            Viaje viaje = CrearViaje();

            viaje.AgregarParticipante(
                CrearPersona(1));

            Assert.ThrowsException<ReglaNegocioException>(
                () => viaje.AgregarParticipante(
                    CrearPersona(1)));
        }

        [TestMethod]
        public void Participantes_NoExponeListaModificable()
        {
            Viaje viaje = CrearViaje();

            viaje.AgregarParticipante(
                CrearPersona(1));

            Assert.IsFalse(
                viaje.Participantes
                    is List<Persona>);
        }

        [TestMethod]
        public void QuitarParticipante_Ultimo_RechazaOperacion()
        {
            Viaje viaje = CrearViaje();
            Persona persona = CrearPersona(1);

            viaje.AgregarParticipante(
                persona);

            Assert.ThrowsException<ReglaNegocioException>(
                () => viaje.QuitarParticipante(
                    persona));
        }

        [TestMethod]
        public void ReemplazarParticipantes_Vacio_RechazaOperacion()
        {
            Viaje viaje = CrearViaje();

            Assert.ThrowsException<ReglaNegocioException>(
                () => viaje.ReemplazarParticipantes(
                    new List<Persona>()));
        }

        [TestMethod]
        public void ActualizarDatos_EnAbierto_ActualizaViaje()
        {
            Viaje viaje = CrearViaje();

            viaje.ReemplazarParticipantes(
                new[]
                {
                    CrearPersona(1)
                });

            viaje.ActualizarDatos(
                new DateTime(2026, 8, 1),
                new DateTime(2026, 8, 3),
                "Viaje modificado",
                TipoViaje.EventoFeria,
                1500m,
                new[]
                {
                    CrearPersona(2),
                    CrearPersona(3)
                });

            Assert.AreEqual(
                "Viaje modificado",
                viaje.Descripcion);

            Assert.AreEqual(
                TipoViaje.EventoFeria,
                viaje.TipoViaje);

            Assert.AreEqual(
                1500m,
                viaje.MontoAnticipado);

            Assert.AreEqual(
                2,
                viaje.Participantes.Count);

            Assert.AreEqual(
                EstadoViaje.Abierto,
                viaje.EstadoActual);
        }

        [TestMethod]
        public void ActualizarDatos_EnRendicion_RechazaOperacion()
        {
            Viaje viaje = CrearViaje();

            viaje.ReemplazarParticipantes(
                new[]
                {
                    CrearPersona(1)
                });

            viaje.EnviarARendicion();

            Assert.ThrowsException<ReglaNegocioException>(
                () => viaje.ActualizarDatos(
                    viaje.FechaInicio,
                    viaje.FechaFin,
                    "Cambio",
                    viaje.TipoViaje,
                    viaje.MontoAnticipado,
                    viaje.Participantes));
        }

        [TestMethod]
        public void ActualizarDatos_PeriodoExcluyeViatico_RechazaOperacion()
        {
            Viaje viaje = CrearViaje();

            viaje.ReemplazarParticipantes(
                new[]
                {
                    CrearPersona(1)
                });

            viaje.AgregarViatico(
                new Viatico(
                    1,
                    new DateTime(2026, 7, 11),
                    100m,
                    "Gasto"));

            Assert.ThrowsException<ReglaNegocioException>(
                () => viaje.ActualizarDatos(
                    new DateTime(2026, 7, 12),
                    new DateTime(2026, 7, 13),
                    "Cambio",
                    TipoViaje.Desplazamiento,
                    0m,
                    viaje.Participantes));
        }

        [TestMethod]
        public void Reconstruir_ParticipanteInactivo_ConservaHistorial()
        {
            Persona persona = CrearPersona(1);
            persona.Desactivar();

            Viaje viaje =
                Viaje.Reconstruir(
                    10,
                    new DateTime(2026, 7, 10),
                    new DateTime(2026, 7, 12),
                    "Viaje histórico",
                    TipoViaje.EnOficina,
                    500m,
                    EstadoViaje.Cancelado,
                    new[]
                    {
                        persona
                    });

            Assert.AreEqual(
                EstadoViaje.Cancelado,
                viaje.EstadoActual);

            Assert.AreEqual(
                1,
                viaje.Participantes.Count);

            Assert.IsFalse(
                persona.Activo);
        }

        private static Viaje CrearViaje()
        {
            return new Viaje(
                1,
                new DateTime(2026, 7, 10),
                new DateTime(2026, 7, 12),
                "Viaje comercial",
                TipoViaje.Desplazamiento,
                0m);
        }

        private static Persona CrearPersona(
            int idPersona)
        {
            return new Persona(
                idPersona,
                "Persona",
                "Prueba " + idPersona,
                "persona" +
                idPersona +
                "@correo.com");
        }
    }
}
