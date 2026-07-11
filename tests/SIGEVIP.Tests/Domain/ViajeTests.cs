using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class ViajeTests
    {
        [TestMethod]
        public void CrearViaje_NaceEnEstadoAbierto()
        {
            Viaje viaje = CrearViaje();

            Assert.AreEqual(
                EstadoViaje.Abierto,
                viaje.EstadoActual);
        }

        [TestMethod]
        public void CrearViaje_ConFechaInicioPosteriorAFechaFin_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viaje(
                        1,
                        new DateTime(2026, 7, 15),
                        new DateTime(2026, 7, 10),
                        "Viaje inválido",
                        TipoViaje.Desplazamiento,
                        0m));

            StringAssert.Contains(
                excepcion.Message,
                "fecha de inicio");
        }

        [TestMethod]
        public void CrearViaje_AceptaTipoEventoFeria()
        {
            var viaje = new Viaje(
                1,
                new DateTime(2026, 7, 10),
                new DateTime(2026, 7, 12),
                "Exposición agrocomercial",
                TipoViaje.EventoFeria,
                0m);

            Assert.AreEqual(
                TipoViaje.EventoFeria,
                viaje.TipoViaje);
        }

        [TestMethod]
        public void CrearViaje_ConTipoNoDefinido_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viaje(
                        1,
                        new DateTime(2026, 7, 10),
                        new DateTime(2026, 7, 12),
                        "Tipo inválido",
                        (TipoViaje)999,
                        0m));

            StringAssert.Contains(
                excepcion.Message,
                "tipo de viaje");
        }

        [TestMethod]
        public void CrearViaje_ConMontoAnticipadoNegativo_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viaje(
                        1,
                        new DateTime(2026, 7, 10),
                        new DateTime(2026, 7, 12),
                        "Viaje inválido",
                        TipoViaje.Desplazamiento,
                        -1m));

            StringAssert.Contains(
                excepcion.Message,
                "monto anticipado");
        }

        [TestMethod]
        public void AgregarViatico_EnViajeAbierto_AgregaViaticoValido()
        {
            Viaje viaje = CrearViaje();
            Viatico viatico = CrearViatico(1, 2500m);

            viaje.AgregarViatico(viatico);

            Assert.AreEqual(1, viaje.Viaticos.Count);
            Assert.AreEqual(viaje.IdViaje, viatico.IdViaje);
        }

        [TestMethod]
        public void AgregarViatico_ConFechaAnteriorAlViaje_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();

            var viatico = new Viatico(
                1,
                new DateTime(2026, 7, 9),
                1000m,
                "Gasto anterior");

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarViatico(viatico));

            StringAssert.Contains(
                excepcion.Message,
                "dentro del período");
        }

        [TestMethod]
        public void AgregarViatico_ConFechaPosteriorAlViaje_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();

            var viatico = new Viatico(
                1,
                new DateTime(2026, 7, 13),
                1000m,
                "Gasto posterior");

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarViatico(viatico));

            StringAssert.Contains(
                excepcion.Message,
                "dentro del período");
        }

        [TestMethod]
        public void AgregarViatico_Nulo_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarViatico(null));

            StringAssert.Contains(
                excepcion.Message,
                "viático válido");
        }

        [TestMethod]
        public void AgregarViatico_Repetido_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();
            Viatico viatico = CrearViatico(1, 1000m);

            viaje.AgregarViatico(viatico);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarViatico(viatico));

            StringAssert.Contains(
                excepcion.Message,
                "ya fue agregado");
        }

        [TestMethod]
        public void Viaticos_NoPermiteModificarColeccionDesdeElExterior()
        {
            Viaje viaje = CrearViaje();

            Assert.IsFalse(
                viaje.Viaticos is List<Viatico>);
        }

        [TestMethod]
        public void TotalGastado_SumaSolamenteViaticosVigentes()
        {
            Viaje viaje = CrearViaje();
            Viatico primero = CrearViatico(1, 1000m);
            Viatico segundo = CrearViatico(2, 2500m);

            viaje.AgregarViatico(primero);
            viaje.AgregarViatico(segundo);
            viaje.EnviarARendicion();
            viaje.ExcluirViatico(segundo);

            Assert.AreEqual(1000m, viaje.TotalGastado);
        }

        [TestMethod]
        public void TotalGastado_SinViaticosVigentes_DevuelveCero()
        {
            Viaje viaje = CrearViaje();
            Viatico viatico = CrearViatico(1, 1000m);

            viaje.AgregarViatico(viatico);
            viaje.EnviarARendicion();
            viaje.ExcluirViatico(viatico);

            Assert.AreEqual(0m, viaje.TotalGastado);
        }

        [TestMethod]
        public void TotalGastado_SinViaticos_DevuelveCero()
        {
            Viaje viaje = CrearViaje();

            Assert.AreEqual(0m, viaje.TotalGastado);
        }

        [TestMethod]
        public void CalcularSaldo_ConResultadoPositivo_DevuelveDiferencia()
        {
            Viaje viaje = CrearViaje(1000m);
            viaje.AgregarViatico(CrearViatico(1, 1500m));

            Assert.AreEqual(500m, viaje.CalcularSaldo());
        }

        [TestMethod]
        public void CalcularSaldo_ConResultadoNegativo_DevuelveDiferencia()
        {
            Viaje viaje = CrearViaje(2000m);
            viaje.AgregarViatico(CrearViatico(1, 1500m));

            Assert.AreEqual(-500m, viaje.CalcularSaldo());
        }

        [TestMethod]
        public void CalcularSaldo_ConResultadoCero_DevuelveCero()
        {
            Viaje viaje = CrearViaje(1500m);
            viaje.AgregarViatico(CrearViatico(1, 1500m));

            Assert.AreEqual(0m, viaje.CalcularSaldo());
        }

        [TestMethod]
        public void EnviarARendicion_DesdeAbierto_CambiaEstado()
        {
            Viaje viaje = CrearViaje();

            viaje.EnviarARendicion();

            Assert.AreEqual(
                EstadoViaje.EnRendicion,
                viaje.EstadoActual);
        }

        [TestMethod]
        public void AgregarViatico_EnRendicion_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();
            viaje.EnviarARendicion();

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarViatico(
                        CrearViatico(1, 1000m)));

            StringAssert.Contains(
                excepcion.Message,
                "no admite modificaciones");
        }

        [TestMethod]
        public void Aprobar_DesdeEnRendicion_CambiaEstado()
        {
            Viaje viaje = CrearViaje();
            viaje.EnviarARendicion();

            viaje.Aprobar();

            Assert.AreEqual(
                EstadoViaje.Aprobado,
                viaje.EstadoActual);
        }

        [TestMethod]
        public void Aprobar_DirectamenteDesdeAbierto_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.Aprobar());

            StringAssert.Contains(
                excepcion.Message,
                "todavía se encuentra Abierto");
        }

        [TestMethod]
        public void ViajeAprobado_NoAdmiteAgregarViaticos()
        {
            Viaje viaje = CrearViaje();
            viaje.EnviarARendicion();
            viaje.Aprobar();

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarViatico(
                        CrearViatico(1, 1000m)));

            StringAssert.Contains(
                excepcion.Message,
                "Aprobado");
        }

        [TestMethod]
        public void ViajeCancelado_NoAdmiteAgregarViaticos()
        {
            Viaje viaje = CrearViaje();
            viaje.Cancelar();

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.AgregarViatico(
                        CrearViatico(1, 1000m)));

            StringAssert.Contains(
                excepcion.Message,
                "Cancelado");
        }

        [TestMethod]
        public void Cancelar_DesdeAbierto_CambiaEstado()
        {
            Viaje viaje = CrearViaje();

            viaje.Cancelar();

            Assert.AreEqual(
                EstadoViaje.Cancelado,
                viaje.EstadoActual);
        }

        [TestMethod]
        public void Cancelar_DesdeEnRendicion_CambiaEstado()
        {
            Viaje viaje = CrearViaje();
            viaje.EnviarARendicion();

            viaje.Cancelar();

            Assert.AreEqual(
                EstadoViaje.Cancelado,
                viaje.EstadoActual);
        }

        [TestMethod]
        public void Cancelar_DesdeAprobado_LanzaExcepcionDeDominio()
        {
            Viaje viaje = CrearViaje();
            viaje.EnviarARendicion();
            viaje.Aprobar();

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.Cancelar());

            StringAssert.Contains(
                excepcion.Message,
                "no puede ser cancelado");
        }

        [TestMethod]
        public void EnviarARendicion_DosVeces_LanzaExcepcionDeDominio()
        {
            Viaje viaje = CrearViaje();
            viaje.EnviarARendicion();

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.EnviarARendicion());

            StringAssert.Contains(
                excepcion.Message,
                "ya se encuentra EnRendicion");
        }

        [TestMethod]
        public void ExcluirViatico_EnRendicion_CambiaEstadoAExcluido()
        {
            Viaje viaje = CrearViaje();
            Viatico viatico = CrearViatico(1, 1000m);

            viaje.AgregarViatico(viatico);
            viaje.EnviarARendicion();
            viaje.ExcluirViatico(viatico);

            Assert.AreEqual(
                EstadoViatico.Excluido,
                viatico.Estado);

            Assert.IsFalse(viatico.EstaVigente);
        }

        [TestMethod]
        public void ReactivarViatico_EnRendicion_CambiaEstadoAVigente()
        {
            Viaje viaje = CrearViaje();
            Viatico viatico = CrearViatico(1, 1000m);

            viaje.AgregarViatico(viatico);
            viaje.EnviarARendicion();
            viaje.ExcluirViatico(viatico);
            viaje.ReactivarViatico(viatico);

            Assert.AreEqual(
                EstadoViatico.Vigente,
                viatico.Estado);

            Assert.IsTrue(viatico.EstaVigente);
        }

        [TestMethod]
        public void ExcluirViatico_EnViajeAbierto_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();
            Viatico viatico = CrearViatico(1, 1000m);
            viaje.AgregarViatico(viatico);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.ExcluirViatico(viatico));

            StringAssert.Contains(
                excepcion.Message,
                "solo pueden excluirse");
        }

        [TestMethod]
        public void ExcluirViatico_QueNoPerteneceAlViaje_LanzaExcepcion()
        {
            Viaje viaje = CrearViaje();
            Viatico viaticoAjeno = CrearViatico(99, 1000m);

            viaje.EnviarARendicion();

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => viaje.ExcluirViatico(viaticoAjeno));

            StringAssert.Contains(
                excepcion.Message,
                "no pertenece al viaje");
        }

        [TestMethod]
        public void ReconstruirViaje_DesdeEstadoPersistible_RecuperaEstado()
        {
            var viaje = new Viaje(
                1,
                new DateTime(2026, 7, 10),
                new DateTime(2026, 7, 12),
                "Viaje reconstruido",
                TipoViaje.EnOficina,
                0m,
                EstadoViaje.EnRendicion);

            Assert.AreEqual(
                EstadoViaje.EnRendicion,
                viaje.EstadoActual);
        }

        [TestMethod]
        public void ReconstruirViaje_ConEstadoNoDefinido_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Viaje(
                        1,
                        new DateTime(2026, 7, 10),
                        new DateTime(2026, 7, 12),
                        "Estado inválido",
                        TipoViaje.EnOficina,
                        0m,
                        (EstadoViaje)999));

            StringAssert.Contains(
                excepcion.Message,
                "estado indicado");
        }

        private static Viaje CrearViaje(decimal montoAnticipado = 0m)
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
                monto,
                "Gasto de prueba");
        }
    }
}
