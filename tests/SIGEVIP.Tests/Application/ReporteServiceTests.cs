using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Reportes;
using SIGEVIP.Application.Rendiciones;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class ReporteServiceTests
    {
        [TestMethod]
        public void ObtenerResumen_SinSesion_RechazaOperacion()
        {
            ReporteService servicio =
                CrearServicio(
                    null);

            Assert.ThrowsException
                <AccesoDenegadoException>(
                    () =>
                        servicio.ObtenerResumenViaje(
                            1));
        }

        [TestMethod]
        public void ObtenerResumen_SinPermiso_RechazaOperacion()
        {
            ReporteService servicio =
                CrearServicio(
                    CrearUsuario());

            Assert.ThrowsException
                <AccesoDenegadoException>(
                    () =>
                        servicio.ObtenerResumenViaje(
                            1));
        }

        [TestMethod]
        public void ObtenerResumen_CalculaIndicadores()
        {
            ReporteRepositoryFalso repository =
                new ReporteRepositoryFalso();

            repository.DatosViaje =
                CrearDatosViaje(
                    1000m,
                    new[]
                    {
                        CrearViatico(
                            1,
                            300m,
                            EstadoViatico.Vigente,
                            CategoriaGasto.Transporte),
                        CrearViatico(
                            2,
                            200m,
                            EstadoViatico.Excluido,
                            CategoriaGasto.Otros)
                    });

            ReporteService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar),
                    repository);

            ReporteViajeResumenDto resultado =
                servicio.ObtenerResumenViaje(
                    1);

            Assert.AreEqual(
                2,
                resultado.CantidadParticipantes);

            Assert.AreEqual(
                2,
                resultado.CantidadVisitas);

            Assert.AreEqual(
                2,
                resultado.CantidadClientesDistintos);

            Assert.AreEqual(
                2,
                resultado.CantidadViaticos);

            Assert.AreEqual(
                500m,
                resultado.TotalRegistrado);

            Assert.AreEqual(
                300m,
                resultado.TotalVigente);

            Assert.AreEqual(
                200m,
                resultado.TotalExcluido);

            Assert.AreEqual(
                700m,
                resultado.DiferenciaAnticipo);

            Assert.AreEqual(
                "Importe a devolver",
                resultado.TipoDiferencia);

            Assert.AreEqual(
                700m,
                resultado.ImporteDiferencia);
        }

        [TestMethod]
        public void ObtenerResumen_GastoSuperiorAlAnticipo_IndicaReintegro()
        {
            ReporteRepositoryFalso repository =
                new ReporteRepositoryFalso();

            repository.DatosViaje =
                CrearDatosViaje(
                    100m,
                    new[]
                    {
                        CrearViatico(
                            1,
                            350m,
                            EstadoViatico.Vigente,
                            CategoriaGasto.Transporte)
                    });

            ReporteService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar),
                    repository);

            ReporteViajeResumenDto resultado =
                servicio.ObtenerResumenViaje(
                    1);

            Assert.AreEqual(
                -250m,
                resultado.DiferenciaAnticipo);

            Assert.AreEqual(
                "Importe a reintegrar",
                resultado.TipoDiferencia);

            Assert.AreEqual(
                250m,
                resultado.ImporteDiferencia);
        }

        [TestMethod]
        public void ObtenerResumen_TarjetaCorporativa_NoAfectaSaldo()
        {
            ReporteRepositoryFalso repository =
                new ReporteRepositoryFalso();

            repository.DatosViaje =
                CrearDatosViaje(
                    1000m,
                    new[]
                    {
                        CrearViatico(
                            1,
                            300m,
                            EstadoViatico.Vigente,
                            CategoriaGasto.Transporte,
                            MetodoPago.PagoPersonal),
                        CrearViatico(
                            2,
                            900m,
                            EstadoViatico.Vigente,
                            CategoriaGasto.Alojamiento,
                            MetodoPago.TarjetaCorporativa)
                    });

            ReporteService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar),
                    repository);

            ReporteViajeResumenDto resultado =
                servicio.ObtenerResumenViaje(
                    1);

            Assert.AreEqual(
                1200m,
                resultado.TotalGastos);

            Assert.AreEqual(
                300m,
                resultado.GastosComputablesSaldo);

            Assert.AreEqual(
                700m,
                resultado.Saldo);

            Assert.AreEqual(
                "Importe a devolver",
                resultado.TipoSaldo);
        }

        [TestMethod]
        public void ObtenerResumen_ViajeInexistente_RechazaOperacion()
        {
            ReporteService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar));

            Assert.ThrowsException
                <ReglaNegocioException>(
                    () =>
                        servicio.ObtenerResumenViaje(
                            1));
        }

        [TestMethod]
        public void ConsultarViaticos_ConPermisoViatico_CalculaResultado()
        {
            ReporteRepositoryFalso repository =
                new ReporteRepositoryFalso();

            repository.Viaticos.Add(
                CrearFila(
                    1,
                    300m,
                    EstadoViatico.Vigente,
                    CategoriaGasto.Transporte));

            repository.Viaticos.Add(
                CrearFila(
                    2,
                    100m,
                    EstadoViatico.Vigente,
                    CategoriaGasto.Transporte));

            repository.Viaticos.Add(
                CrearFila(
                    3,
                    250m,
                    EstadoViatico.Vigente,
                    CategoriaGasto.Otros));

            repository.Viaticos.Add(
                CrearFila(
                    4,
                    50m,
                    EstadoViatico.Excluido,
                    CategoriaGasto.Otros));

            ReporteService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoConsultar),
                    repository);

            ReporteViaticoResultadoDto resultado =
                servicio.ConsultarViaticos(
                    ReporteViaticoFiltro
                        .CrearSinFiltros());

            Assert.AreEqual(
                4,
                resultado.Cantidad);

            Assert.AreEqual(
                700m,
                resultado.TotalRegistrado);

            Assert.AreEqual(
                650m,
                resultado.TotalVigente);

            Assert.AreEqual(
                50m,
                resultado.TotalExcluido);

            Assert.AreEqual(
                175m,
                resultado.Promedio);

            Assert.AreEqual(
                CategoriaGasto.Transporte,
                resultado.CategoriaMayorGasto);

            Assert.AreEqual(
                400m,
                resultado.TotalCategoriaMayorGasto);
        }

        [TestMethod]
        public void ConsultarViaticos_ConPermisoRendicion_AutorizaOperacion()
        {
            ReporteRepositoryFalso repository =
                new ReporteRepositoryFalso();

            ReporteService servicio =
                CrearServicio(
                    CrearUsuario(
                        RendicionService.PermisoRevisar),
                    repository);

            ReporteViaticoResultadoDto resultado =
                servicio.ConsultarViaticos(
                    null);

            Assert.AreEqual(
                0,
                resultado.Cantidad);

            Assert.AreEqual(
                0m,
                resultado.Promedio);
        }

        [TestMethod]
        public void ConsultarViaticos_SinPermisoFinanciero_RechazaOperacion()
        {
            ReporteService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar));

            Assert.ThrowsException
                <AccesoDenegadoException>(
                    () =>
                        servicio.ConsultarViaticos(
                            null));
        }

        [TestMethod]
        public void ConsultarViaticos_TransfiereFiltro()
        {
            ReporteRepositoryFalso repository =
                new ReporteRepositoryFalso();

            ReporteViaticoFiltro filtro =
                new ReporteViaticoFiltro(
                    new DateTime(
                        2026,
                        8,
                        1),
                    new DateTime(
                        2026,
                        8,
                        31),
                    7,
                    9,
                    CategoriaGasto.Transporte,
                    MetodoPago.PagoPersonal,
                    EstadoViatico.Vigente,
                    EstadoViaje.Aprobado);

            ReporteService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViaticoService.PermisoConsultar),
                    repository);

            servicio.ConsultarViaticos(
                filtro);

            Assert.AreSame(
                filtro,
                repository.UltimoFiltro);
        }

        [TestMethod]
        public void ConsultarAnalisis_Clientes_CalculaRankingYTop()
        {
            ReporteRepositoryFalso repository =
                new ReporteRepositoryFalso();

            repository.Analisis.Add(
                new ReporteAnalisisItemDto(
                    "Cliente A",
                    12m));

            repository.Analisis.Add(
                new ReporteAnalisisItemDto(
                    "Cliente B",
                    8m));

            repository.Analisis.Add(
                new ReporteAnalisisItemDto(
                    "Cliente C",
                    5m));

            ReporteService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar),
                    repository);

            ReporteAnalisisResultadoDto resultado =
                servicio.ConsultarAnalisis(
                    new ReporteAnalisisFiltro(
                        null,
                        null,
                        ReporteAreaAnalisis.Clientes,
                        ReporteIndicadorAnalisis.CantidadVisitas,
                        ReporteAgrupacionAnalisis.Cliente,
                        5));

            Assert.AreEqual(
                25m,
                resultado.Total);

            Assert.AreEqual(
                12m,
                resultado.Maximo);

            Assert.AreEqual(
                "Cliente A",
                resultado.ElementoDestacado);

            Assert.AreEqual(
                3,
                resultado.Items.Count);

            Assert.AreEqual(
                48m,
                resultado.Items
                    .First()
                    .Participacion);
        }

        [TestMethod]
        public void ConsultarAnalisis_Gastos_SinPermisoFinanciero_RechazaOperacion()
        {
            ReporteService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar));

            Assert.ThrowsException
                <AccesoDenegadoException>(
                    () =>
                        servicio.ConsultarAnalisis(
                            new ReporteAnalisisFiltro(
                                null,
                                null,
                                ReporteAreaAnalisis.Gastos,
                                ReporteIndicadorAnalisis.Importe,
                                ReporteAgrupacionAnalisis.Categoria,
                                10)));
        }

        [TestMethod]
        public void ConsultarAnalisis_EmpleadosViajes_ConPermisoResumen_Autoriza()
        {
            ReporteRepositoryFalso repository =
                new ReporteRepositoryFalso();

            repository.Analisis.Add(
                new ReporteAnalisisItemDto(
                    "Empleado",
                    2m));

            ReporteService servicio =
                CrearServicio(
                    CrearUsuario(
                        ViajeService.PermisoConsultar),
                    repository);

            ReporteAnalisisResultadoDto resultado =
                servicio.ConsultarAnalisis(
                    new ReporteAnalisisFiltro(
                        null,
                        null,
                        ReporteAreaAnalisis.Empleados,
                        ReporteIndicadorAnalisis.CantidadViajes,
                        ReporteAgrupacionAnalisis.Empleado,
                        10));

            Assert.AreEqual(
                2m,
                resultado.Total);
        }

        private static ReporteService CrearServicio(
            Usuario usuario,
            ReporteRepositoryFalso repository = null)
        {
            return new ReporteService(
                repository
                    ?? new ReporteRepositoryFalso(),
                new SesionActualFalsa(
                    usuario),
                new AutorizacionService());
        }

        private static Usuario CrearUsuario(
            params string[] permisos)
        {
            Usuario usuario =
                new Usuario(
                    1,
                    1,
                    "usuario",
                    new byte[]
                    {
                        1
                    },
                    new byte[]
                    {
                        2
                    },
                    100000);

            Grupo grupo =
                new Grupo(
                    1,
                    "GRUPO_PRUEBA",
                    "Grupo de prueba",
                    string.Empty);

            int idPermiso = 1;

            foreach (
                string codigo
                in permisos)
            {
                grupo.AgregarComponente(
                    new Permiso(
                        idPermiso++,
                        codigo,
                        codigo,
                        string.Empty));
            }

            usuario.AgregarGrupo(
                grupo);

            return usuario;
        }

        private static ReporteViajeDatosDto
            CrearDatosViaje(
                decimal montoAnticipado,
                IEnumerable
                    <ReporteViajeViaticoDto>
                    viaticos)
        {
            return new ReporteViajeDatosDto(
                1,
                "Viaje comercial",
                TipoViaje.Desplazamiento,
                new DateTime(
                    2026,
                    8,
                    1),
                new DateTime(
                    2026,
                    8,
                    5),
                EstadoViaje.Abierto,
                montoAnticipado,
                string.Empty,
                null,
                string.Empty,
                null,
                string.Empty,
                null,
                string.Empty,
                new[]
                {
                    new ReporteViajeParticipanteDto(
                        1,
                        "Persona 1",
                        "persona1@sigevip.local",
                        true),
                    new ReporteViajeParticipanteDto(
                        2,
                        "Persona 2",
                        "persona2@sigevip.local",
                        true)
                },
                new[]
                {
                    new ReporteViajeVisitaDto(
                        10,
                        new DateTime(
                            2026,
                            8,
                            2),
                        "Visita 1",
                        "Rosario"),
                    new ReporteViajeVisitaDto(
                        11,
                        new DateTime(
                            2026,
                            8,
                            3),
                        "Visita 2",
                        "Funes")
                },
                new[]
                {
                    new ReporteViajeClienteDto(
                        10,
                        100,
                        "Cliente A",
                        "30-11111111-1",
                        "Rosario",
                        "Santa Fe",
                        true),
                    new ReporteViajeClienteDto(
                        11,
                        100,
                        "Cliente A",
                        "30-11111111-1",
                        "Rosario",
                        "Santa Fe",
                        true),
                    new ReporteViajeClienteDto(
                        11,
                        101,
                        "Cliente B",
                        "30-22222222-2",
                        "Funes",
                        "Santa Fe",
                        true)
                },
                viaticos);
        }

        private static ReporteViajeViaticoDto
            CrearViatico(
                int idViatico,
                decimal monto,
                EstadoViatico estado,
                CategoriaGasto categoria)
        {
            return CrearViatico(
                idViatico,
                monto,
                estado,
                categoria,
                MetodoPago.EfectivoEmpresa);
        }

        private static ReporteViajeViaticoDto
            CrearViatico(
                int idViatico,
                decimal monto,
                EstadoViatico estado,
                CategoriaGasto categoria,
                MetodoPago metodoPago)
        {
            return new ReporteViajeViaticoDto(
                idViatico,
                new DateTime(
                    2026,
                    8,
                    2),
                categoria,
                metodoPago,
                "Persona",
                monto,
                "Gasto",
                estado);
        }

        private static ReporteViaticoFilaDto
            CrearFila(
                int idViatico,
                decimal monto,
                EstadoViatico estado,
                CategoriaGasto categoria)
        {
            return new ReporteViaticoFilaDto(
                idViatico,
                1,
                "Viaje comercial",
                EstadoViaje.Abierto,
                new DateTime(
                    2026,
                    8,
                    2),
                null,
                string.Empty,
                categoria,
                MetodoPago.EfectivoEmpresa,
                monto,
                estado,
                "Gasto");
        }

        private sealed class SesionActualFalsa
            : ISesionActual
        {
            public SesionActualFalsa(
                Usuario usuario)
            {
                UsuarioActual = usuario;
            }

            public bool HayUsuarioAutenticado
            {
                get
                {
                    return UsuarioActual != null;
                }
            }

            public Usuario UsuarioActual
            {
                get;
                private set;
            }

            public void Iniciar(
                Usuario usuario)
            {
                UsuarioActual = usuario;
            }

            public void Cerrar()
            {
                UsuarioActual = null;
            }
        }

        private sealed class ReporteRepositoryFalso
            : IReporteRepository
        {
            public ReporteRepositoryFalso()
            {
                Viajes =
                    new List
                        <ReporteViajeSeleccionDto>();

                Personas =
                    new List
                        <ReportePersonaSeleccionDto>();

                Viaticos =
                    new List
                        <ReporteViaticoFilaDto>();

                Analisis =
                    new List
                        <ReporteAnalisisItemDto>();
            }

            public List<ReporteViajeSeleccionDto>
                Viajes
            {
                get;
                private set;
            }

            public List<ReportePersonaSeleccionDto>
                Personas
            {
                get;
                private set;
            }

            public List<ReporteViaticoFilaDto>
                Viaticos
            {
                get;
                private set;
            }

            public List<ReporteAnalisisItemDto>
                Analisis
            {
                get;
                private set;
            }

            public ReporteViajeDatosDto DatosViaje
            {
                get;
                set;
            }

            public ReporteViaticoFiltro UltimoFiltro
            {
                get;
                private set;
            }

            public IReadOnlyCollection
                <ReporteViajeSeleccionDto>
                ListarViajes()
            {
                return Viajes.AsReadOnly();
            }

            public IReadOnlyCollection
                <ReportePersonaSeleccionDto>
                ListarPersonasPagadoras()
            {
                return Personas.AsReadOnly();
            }

            public ReporteViajeDatosDto
                ObtenerDatosViaje(
                    int idViaje)
            {
                return DatosViaje != null &&
                    DatosViaje.IdViaje == idViaje
                        ? DatosViaje
                        : null;
            }

            public IReadOnlyCollection
                <ReporteViaticoFilaDto>
                ListarViaticos(
                    ReporteViaticoFiltro filtro)
            {
                UltimoFiltro = filtro;
                return Viaticos.AsReadOnly();
            }

            public IReadOnlyCollection
                <ReporteAnalisisItemDto>
                ConsultarAnalisis(
                    ReporteAnalisisFiltro filtro)
            {
                return Analisis.AsReadOnly();
            }
        }
    }
}
