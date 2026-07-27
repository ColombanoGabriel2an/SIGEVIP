using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Rendiciones;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class RendicionApplicationModelTests
    {
        [TestMethod]
        public void ExcluirViaticoCommand_DatosValidos_NormalizaMotivo()
        {
            ExcluirViaticoCommand command =
                new ExcluirViaticoCommand(
                    10,
                    20,
                    "  Comprobante duplicado  ");

            Assert.AreEqual(
                10,
                command.IdViaje);

            Assert.AreEqual(
                20,
                command.IdViatico);

            Assert.AreEqual(
                "Comprobante duplicado",
                command.Motivo);
        }

        [TestMethod]
        public void ExcluirViaticoCommand_SinMotivo_RechazaOperacion()
        {
            Assert.ThrowsException<ReglaNegocioException>(
                () => new ExcluirViaticoCommand(
                    10,
                    20,
                    " "));
        }

        [TestMethod]
        public void AjustarAnticipoCommand_MontoNegativo_RechazaOperacion()
        {
            Assert.ThrowsException<ReglaNegocioException>(
                () => new AjustarAnticipoCommand(
                    10,
                    -1m));
        }

        [TestMethod]
        public void CancelarRendicionCommand_SinMotivo_RechazaOperacion()
        {
            Assert.ThrowsException<ReglaNegocioException>(
                () => new CancelarRendicionCommand(
                    10,
                    null));
        }

        [TestMethod]
        public void RendicionViaticoDto_SinComprobante_InformaFalse()
        {
            RendicionViaticoDto dto =
                new RendicionViaticoDto(
                    1,
                    DateTime.Today,
                    CategoriaGasto.Otros,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    100m,
                    "Gasto",
                    EstadoViatico.Vigente,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null);

            Assert.IsFalse(
                dto.TieneComprobante);

            Assert.AreEqual(
                string.Empty,
                dto.PagadoPor);
        }

        [TestMethod]
        public void RendicionDetalleDto_ColeccionesNulas_CreaColeccionesVacias()
        {
            RendicionDetalleDto dto =
                new RendicionDetalleDto(
                    1,
                    new DateTime(2026, 7, 10),
                    new DateTime(2026, 7, 12),
                    "Viaje",
                    TipoViaje.Desplazamiento,
                    EstadoViaje.EnRendicion,
                    1000m,
                    1500m,
                    500m,
                    null,
                    null,
                    null,
                    5,
                    DateTime.Now);

            Assert.AreEqual(
                0,
                dto.Participantes.Count);

            Assert.AreEqual(
                0,
                dto.Visitas.Count);

            Assert.AreEqual(
                0,
                dto.Viaticos.Count);
        }

        [TestMethod]
        public void RendicionListadoDto_ConSaldoPositivo_ConservaValor()
        {
            RendicionListadoDto dto =
                new RendicionListadoDto(
                    1,
                    new DateTime(2026, 7, 10),
                    new DateTime(2026, 7, 12),
                    "Viaje",
                    TipoViaje.Desplazamiento,
                    "Persona Prueba",
                    1000m,
                    1500m,
                    500m,
                    DateTime.Now);

            Assert.AreEqual(
                500m,
                dto.Saldo);
        }

        [TestMethod]
        public void ReactivarViaticoCommand_IdInvalido_RechazaOperacion()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new ReactivarViaticoCommand(
                    1,
                    0));
        }
    }
}