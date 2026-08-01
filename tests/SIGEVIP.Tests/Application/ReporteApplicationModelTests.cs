using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Reportes;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class ReporteApplicationModelTests
    {
        [TestMethod]
        public void Filtro_FechasValidas_NormalizaFechas()
        {
            ReporteViaticoFiltro filtro =
                new ReporteViaticoFiltro(
                    new DateTime(
                        2026,
                        8,
                        1,
                        15,
                        20,
                        0),
                    new DateTime(
                        2026,
                        8,
                        5,
                        23,
                        0,
                        0),
                    1,
                    2,
                    CategoriaGasto.Transporte,
                    MetodoPago.PagoPersonal,
                    EstadoViatico.Vigente,
                    EstadoViaje.Abierto);

            Assert.AreEqual(
                new DateTime(
                    2026,
                    8,
                    1),
                filtro.FechaDesde);

            Assert.AreEqual(
                new DateTime(
                    2026,
                    8,
                    5),
                filtro.FechaHasta);
        }

        [TestMethod]
        public void Filtro_FechaDesdePosterior_RechazaOperacion()
        {
            Assert.ThrowsException
                <ReglaNegocioException>(
                    () =>
                        new ReporteViaticoFiltro(
                            new DateTime(
                                2026,
                                8,
                                5),
                            new DateTime(
                                2026,
                                8,
                                1),
                            null,
                            null,
                            null,
                            null,
                            null,
                            null));
        }

        [TestMethod]
        public void Filtro_IdViajeInvalido_RechazaOperacion()
        {
            Assert.ThrowsException
                <ArgumentOutOfRangeException>(
                    () =>
                        new ReporteViaticoFiltro(
                            null,
                            null,
                            0,
                            null,
                            null,
                            null,
                            null,
                            null));
        }

        [TestMethod]
        public void Filtro_EnumInvalido_RechazaOperacion()
        {
            Assert.ThrowsException
                <ReglaNegocioException>(
                    () =>
                        new ReporteViaticoFiltro(
                            null,
                            null,
                            null,
                            null,
                            (CategoriaGasto)255,
                            null,
                            null,
                            null));
        }

        [TestMethod]
        public void AnalisisFiltro_FechasValidas_NormalizaFechas()
        {
            ReporteAnalisisFiltro filtro =
                new ReporteAnalisisFiltro(
                    new DateTime(
                        2026,
                        7,
                        1,
                        18,
                        30,
                        0),
                    new DateTime(
                        2026,
                        7,
                        31,
                        23,
                        59,
                        0),
                    ReporteAreaAnalisis.Clientes,
                    ReporteIndicadorAnalisis.CantidadVisitas,
                    ReporteAgrupacionAnalisis.Cliente,
                    10);

            Assert.AreEqual(
                new DateTime(
                    2026,
                    7,
                    1),
                filtro.FechaDesde);

            Assert.AreEqual(
                new DateTime(
                    2026,
                    7,
                    31),
                filtro.FechaHasta);
        }

        [TestMethod]
        public void AnalisisFiltro_CombinacionInvalida_RechazaOperacion()
        {
            Assert.ThrowsException
                <ReglaNegocioException>(
                    () =>
                        new ReporteAnalisisFiltro(
                            null,
                            null,
                            ReporteAreaAnalisis.Clientes,
                            ReporteIndicadorAnalisis.Importe,
                            ReporteAgrupacionAnalisis.Cliente,
                            10));
        }

        [TestMethod]
        public void AnalisisFiltro_TopInvalido_RechazaOperacion()
        {
            Assert.ThrowsException
                <ReglaNegocioException>(
                    () =>
                        new ReporteAnalisisFiltro(
                            null,
                            null,
                            ReporteAreaAnalisis.Viajes,
                            ReporteIndicadorAnalisis.CantidadViajes,
                            ReporteAgrupacionAnalisis.Mes,
                            7));
        }

        [TestMethod]
        public void Fila_SinPagador_ExponeTextoDeNegocio()
        {
            ReporteViaticoFilaDto fila =
                new ReporteViaticoFilaDto(
                    1,
                    2,
                    "Viaje",
                    EstadoViaje.Abierto,
                    new DateTime(
                        2026,
                        8,
                        1),
                    null,
                    string.Empty,
                    CategoriaGasto.Otros,
                    MetodoPago.EfectivoEmpresa,
                    100m,
                    EstadoViatico.Vigente,
                    "Gasto");

            Assert.AreEqual(
                "Sin persona pagadora",
                fila.PersonaPagadora);
        }
    }
}
