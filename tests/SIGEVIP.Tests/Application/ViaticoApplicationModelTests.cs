using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class ViaticoApplicationModelTests
    {
        [TestMethod]
        public void ViaticoFiltro_FechasValidas_NormalizaFechas()
        {
            ViaticoFiltro filtro =
                new ViaticoFiltro(
                    new DateTime(
                        2026,
                        7,
                        10,
                        15,
                        30,
                        0),
                    new DateTime(
                        2026,
                        7,
                        12,
                        20,
                        0,
                        0),
                    CategoriaGasto.Alimentacion,
                    EstadoViatico.Vigente);

            Assert.AreEqual(
                new DateTime(2026, 7, 10),
                filtro.FechaDesde);

            Assert.AreEqual(
                new DateTime(2026, 7, 12),
                filtro.FechaHasta);
        }

        [TestMethod]
        public void ViaticoFiltro_FechaDesdePosterior_RechazaOperacion()
        {
            Assert.ThrowsException<ReglaNegocioException>(
                () => new ViaticoFiltro(
                    new DateTime(2026, 7, 12),
                    new DateTime(2026, 7, 10),
                    null,
                    null));
        }

        [TestMethod]
        public void ViaticoFiltro_CategoriaInvalida_RechazaOperacion()
        {
            Assert.ThrowsException<ReglaNegocioException>(
                () => new ViaticoFiltro(
                    null,
                    null,
                    (CategoriaGasto)999,
                    null));
        }

        [TestMethod]
        public void RegistrarViaticoCommand_DatosValidos_ConservaValores()
        {
            ComprobanteInput comprobante =
                CrearComprobanteInput();

            RegistrarViaticoCommand command =
                new RegistrarViaticoCommand(
                    10,
                    new DateTime(2026, 7, 11),
                    CategoriaGasto.Alimentacion,
                    MetodoPago.PagoPersonal,
                    5,
                    1500m,
                    "Almuerzo",
                    comprobante);

            Assert.AreEqual(
                10,
                command.IdViaje);

            Assert.AreEqual(
                5,
                command.IdPersonaPagadora);

            Assert.AreEqual(
                1500m,
                command.Monto);

            Assert.AreSame(
                comprobante,
                command.Comprobante);
        }

        [TestMethod]
        public void RegistrarViaticoCommand_IdViajeInvalido_RechazaOperacion()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new RegistrarViaticoCommand(
                    0,
                    DateTime.Today,
                    CategoriaGasto.Otros,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    100m,
                    "Gasto",
                    null));
        }

        [TestMethod]
        public void ModificarViaticoCommand_IdViaticoInvalido_RechazaOperacion()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new ModificarViaticoCommand(
                    1,
                    0,
                    DateTime.Today,
                    CategoriaGasto.Otros,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    100m,
                    "Gasto",
                    null));
        }

        [TestMethod]
        public void ComprobanteInput_EnumInvalido_RechazaOperacion()
        {
            Assert.ThrowsException<ReglaNegocioException>(
                () => new ComprobanteInput(
                    0,
                    (TipoComprobante)999,
                    "30-12345678-9",
                    "Proveedor",
                    SituacionFiscal.ResponsableInscripto,
                    "0001",
                    "00001234",
                    100m,
                    21m));
        }

        [TestMethod]
        public void ViaticoListadoDto_ValoresNulos_UtilizaCadenasVacias()
        {
            ViaticoListadoDto dto =
                new ViaticoListadoDto(
                    1,
                    10,
                    DateTime.Today,
                    CategoriaGasto.Otros,
                    MetodoPago.EfectivoEmpresa,
                    null,
                    100m,
                    null,
                    EstadoViatico.Vigente,
                    false);

            Assert.AreEqual(
                string.Empty,
                dto.PagadoPor);

            Assert.AreEqual(
                string.Empty,
                dto.Descripcion);
        }

        private static ComprobanteInput
            CrearComprobanteInput()
        {
            return new ComprobanteInput(
                0,
                TipoComprobante.FacturaB,
                "30-12345678-9",
                "Proveedor",
                SituacionFiscal.ResponsableInscripto,
                "0001",
                "00001234",
                1000m,
                210m);
        }
    }
}