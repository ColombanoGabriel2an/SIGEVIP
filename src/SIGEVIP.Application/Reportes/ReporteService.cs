using System;
using System.Collections.Generic;
using System.Linq;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Rendiciones;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteService
    {
        public const string PermisoResumenViaje =
            ViajeService.PermisoConsultar;

        public const string PermisoReporteViaticos =
            ViaticoService.PermisoConsultar;

        public const string PermisoReporteRendiciones =
            RendicionService.PermisoRevisar;

        private readonly IReporteRepository
            _reporteRepository;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        public ReporteService(
            IReporteRepository reporteRepository,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _reporteRepository =
                reporteRepository
                ?? throw new ArgumentNullException(
                    nameof(reporteRepository));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));

            _autorizacionService =
                autorizacionService
                ?? throw new ArgumentNullException(
                    nameof(autorizacionService));
        }

        public IReadOnlyCollection
            <ReporteViajeSeleccionDto>
            ListarViajesParaResumen()
        {
            ExigirPermiso(
                PermisoResumenViaje);

            return NormalizarColeccion(
                _reporteRepository
                    .ListarViajes());
        }

        public IReadOnlyCollection
            <ReporteViajeSeleccionDto>
            ListarViajesParaReporteViaticos()
        {
            ExigirPermisoFinanciero();

            return NormalizarColeccion(
                _reporteRepository
                    .ListarViajes());
        }

        public IReadOnlyCollection
            <ReportePersonaSeleccionDto>
            ListarPersonasPagadoras()
        {
            ExigirPermisoFinanciero();

            return NormalizarColeccion(
                _reporteRepository
                    .ListarPersonasPagadoras());
        }

        public ReporteViajeResumenDto
            ObtenerResumenViaje(
                int idViaje)
        {
            ExigirPermiso(
                PermisoResumenViaje);

            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje),
                    "El identificador del Viaje debe ser mayor que cero.");
            }

            ReporteViajeDatosDto datos =
                _reporteRepository
                    .ObtenerDatosViaje(
                        idViaje);

            if (datos == null)
            {
                throw new ReglaNegocioException(
                    "El Viaje indicado no existe.");
            }

            decimal totalRegistrado =
                datos.Viaticos.Sum(
                    item =>
                        item.Monto);

            decimal totalVigente =
                datos.Viaticos
                    .Where(
                        item =>
                            item.Estado ==
                                EstadoViatico.Vigente)
                    .Sum(
                        item =>
                            item.Monto);

            decimal totalExcluido =
                datos.Viaticos
                    .Where(
                        item =>
                            item.Estado ==
                                EstadoViatico.Excluido)
                    .Sum(
                        item =>
                            item.Monto);

            decimal gastosComputablesSaldo =
                datos.Viaticos
                    .Where(
                        item =>
                            item.Estado ==
                                EstadoViatico.Vigente
                            &&
                            item.MetodoPago !=
                                MetodoPago.TarjetaCorporativa)
                    .Sum(
                        item =>
                            item.Monto);

            decimal saldo =
                datos.MontoAnticipado -
                gastosComputablesSaldo;

            return new ReporteViajeResumenDto(
                datos,
                datos.Participantes.Count,
                datos.Visitas.Count,
                datos.Clientes
                    .Select(
                        item =>
                            item.IdCliente)
                    .Distinct()
                    .Count(),
                datos.Viaticos.Count,
                totalRegistrado,
                totalVigente,
                totalExcluido,
                gastosComputablesSaldo,
                saldo,
                ObtenerTipoDiferencia(
                    saldo));
        }

        public ReporteViaticoResultadoDto
            ConsultarViaticos(
                ReporteViaticoFiltro filtro)
        {
            ExigirPermisoFinanciero();

            ReporteViaticoFiltro
                filtroEfectivo =
                    filtro
                    ?? ReporteViaticoFiltro
                        .CrearSinFiltros();

            IReadOnlyCollection
                <ReporteViaticoFilaDto>
                resultadoRepositorio =
                    _reporteRepository
                        .ListarViaticos(
                            filtroEfectivo);

            List<ReporteViaticoFilaDto> filas =
                (resultadoRepositorio
                    ?? new List
                        <ReporteViaticoFilaDto>())
                .ToList();

            decimal totalRegistrado =
                filas.Sum(
                    item =>
                        item.Monto);

            decimal totalVigente =
                filas
                    .Where(
                        item =>
                            item.EstadoViatico ==
                                EstadoViatico.Vigente)
                    .Sum(
                        item =>
                            item.Monto);

            decimal totalExcluido =
                filas
                    .Where(
                        item =>
                            item.EstadoViatico ==
                                EstadoViatico.Excluido)
                    .Sum(
                        item =>
                            item.Monto);

            decimal promedio =
                filas.Count == 0
                    ? 0m
                    : totalRegistrado /
                        filas.Count;

            List<ReporteCategoriaGastoDto>
                porCategoria =
                    filas
                        .Where(
                            item =>
                                item.EstadoViatico ==
                                    EstadoViatico.Vigente)
                        .GroupBy(
                            item =>
                                item.Categoria)
                        .Select(
                            grupo =>
                                new ReporteCategoriaGastoDto(
                                    grupo.Key,
                                    grupo.Sum(
                                        item =>
                                            item.Monto)))
                        .OrderBy(
                            item =>
                                item.Categoria)
                        .ToList();

            ReporteCategoriaGastoDto mayor =
                porCategoria
                    .OrderByDescending(
                        item =>
                            item.TotalVigente)
                    .ThenBy(
                        item =>
                            item.Categoria)
                    .FirstOrDefault();

            return new ReporteViaticoResultadoDto(
                filas,
                totalRegistrado,
                totalVigente,
                totalExcluido,
                promedio,
                mayor == null
                    ? (CategoriaGasto?)null
                    : mayor.Categoria,
                mayor == null
                    ? 0m
                    : mayor.TotalVigente,
                porCategoria);
        }

        public ReporteAnalisisResultadoDto
            ConsultarAnalisis(
                ReporteAnalisisFiltro filtro)
        {
            if (filtro == null)
            {
                throw new ArgumentNullException(
                    nameof(filtro));
            }

            ExigirPermisoAnalisis(
                filtro);

            IReadOnlyCollection
                <ReporteAnalisisItemDto>
                resultadoRepositorio =
                    _reporteRepository
                        .ConsultarAnalisis(
                            filtro);

            List<ReporteAnalisisItemDto>
                todos =
                    (resultadoRepositorio
                        ?? new List
                            <ReporteAnalisisItemDto>())
                    .Where(
                        item =>
                            item != null &&
                            item.Valor > 0m)
                    .OrderByDescending(
                        item =>
                            item.Valor)
                    .ThenBy(
                        item =>
                            item.Etiqueta)
                    .ToList();

            decimal total =
                todos.Sum(
                    item =>
                        item.Valor);

            decimal maximo =
                todos.Count == 0
                    ? 0m
                    : todos[0].Valor;

            decimal promedio =
                todos.Count == 0
                    ? 0m
                    : total / todos.Count;

            List<ReporteAnalisisItemDto>
                seleccionados =
                    (
                        filtro.Top > 0
                            ? todos.Take(
                                filtro.Top)
                            : todos
                    )
                    .Select(
                        item =>
                            new ReporteAnalisisItemDto(
                                item.Etiqueta,
                                item.Valor,
                                total == 0m
                                    ? 0m
                                    : item.Valor *
                                        100m /
                                        total))
                    .ToList();

            ReporteAnalisisItemDto destacado =
                todos.FirstOrDefault();

            return new ReporteAnalisisResultadoDto(
                ObtenerTituloAnalisis(
                    filtro),
                ObtenerUnidadAnalisis(
                    filtro),
                seleccionados.AsReadOnly(),
                total,
                maximo,
                promedio,
                destacado == null
                    ? string.Empty
                    : destacado.Etiqueta,
                destacado == null
                    ? 0m
                    : destacado.Valor);
        }

        private void ExigirPermisoAnalisis(
            ReporteAnalisisFiltro filtro)
        {
            if (filtro.Area ==
                    ReporteAreaAnalisis.Gastos
                ||
                (
                    filtro.Area ==
                        ReporteAreaAnalisis.Empleados
                    &&
                    filtro.Indicador ==
                        ReporteIndicadorAnalisis.Importe
                ))
            {
                ExigirPermisoFinanciero();
                return;
            }

            ExigirPermiso(
                PermisoResumenViaje);
        }

        private static string ObtenerTituloAnalisis(
            ReporteAnalisisFiltro filtro)
        {
            string baseTitulo;

            switch (filtro.Area)
            {
                case ReporteAreaAnalisis.Gastos:
                    baseTitulo =
                        "Gastos por " +
                        ObtenerNombreAgrupacion(
                            filtro.Agrupacion);
                    break;

                case ReporteAreaAnalisis.Clientes:
                    baseTitulo =
                        "Clientes más visitados";
                    break;

                case ReporteAreaAnalisis.Viajes:
                    baseTitulo =
                        "Cantidad de Viajes por " +
                        ObtenerNombreAgrupacion(
                            filtro.Agrupacion);
                    break;

                case ReporteAreaAnalisis.Empleados:
                    switch (filtro.Indicador)
                    {
                        case ReporteIndicadorAnalisis.Importe:
                            baseTitulo =
                                "Gastos por empleado";
                            break;

                        case ReporteIndicadorAnalisis.CantidadVisitas:
                            baseTitulo =
                                "Visitas por empleado";
                            break;

                        default:
                            baseTitulo =
                                "Viajes por empleado";
                            break;
                    }
                    break;

                default:
                    baseTitulo =
                        "Análisis";
                    break;
            }

            return filtro.Top > 0
                ? baseTitulo +
                    " - Top " +
                    filtro.Top
                : baseTitulo;
        }

        private static string ObtenerUnidadAnalisis(
            ReporteAnalisisFiltro filtro)
        {
            switch (filtro.Indicador)
            {
                case ReporteIndicadorAnalisis.Importe:
                    return "importe";

                case ReporteIndicadorAnalisis.CantidadVisitas:
                    return "visitas";

                case ReporteIndicadorAnalisis.CantidadViajes:
                    return "Viajes";

                default:
                    return string.Empty;
            }
        }

        private static string ObtenerNombreAgrupacion(
            ReporteAgrupacionAnalisis agrupacion)
        {
            switch (agrupacion)
            {
                case ReporteAgrupacionAnalisis.Categoria:
                    return "categoría";

                case ReporteAgrupacionAnalisis.Empleado:
                    return "empleado";

                case ReporteAgrupacionAnalisis.Viaje:
                    return "Viaje";

                case ReporteAgrupacionAnalisis.Mes:
                    return "mes";

                case ReporteAgrupacionAnalisis.Cliente:
                    return "cliente";

                case ReporteAgrupacionAnalisis.Localidad:
                    return "localidad";

                case ReporteAgrupacionAnalisis.Provincia:
                    return "provincia";

                case ReporteAgrupacionAnalisis.TipoViaje:
                    return "tipo de Viaje";

                case ReporteAgrupacionAnalisis.EstadoViaje:
                    return "estado de Viaje";

                default:
                    return "criterio";
            }
        }

        private void ExigirPermisoFinanciero()
        {
            ExigirAlMenosUnPermiso(
                PermisoReporteViaticos,
                PermisoReporteRendiciones);
        }

        private void ExigirPermiso(
            string codigoPermiso)
        {
            ExigirUsuarioValido();

            if (!_autorizacionService
                    .TienePermiso(
                        _sesionActual.UsuarioActual,
                        codigoPermiso))
            {
                throw new AccesoDenegadoException(
                    "No posee permisos para realizar esta operación.");
            }
        }

        private void ExigirAlMenosUnPermiso(
            params string[] codigosPermiso)
        {
            ExigirUsuarioValido();

            bool autorizado =
                codigosPermiso.Any(
                    codigo =>
                        _autorizacionService
                            .TienePermiso(
                                _sesionActual.UsuarioActual,
                                codigo));

            if (!autorizado)
            {
                throw new AccesoDenegadoException(
                    "No posee permisos para realizar esta operación.");
            }
        }

        private void ExigirUsuarioValido()
        {
            if (!_sesionActual.HayUsuarioAutenticado ||
                _sesionActual.UsuarioActual == null)
            {
                throw new AccesoDenegadoException(
                    "Debe iniciar sesión para realizar esta operación.");
            }

            if (!_sesionActual.UsuarioActual.Activo)
            {
                throw new AccesoDenegadoException(
                    "El usuario autenticado se encuentra inactivo.");
            }
        }

        private static string ObtenerTipoDiferencia(
            decimal diferencia)
        {
            if (diferencia > 0m)
            {
                return "Importe a devolver";
            }

            if (diferencia < 0m)
            {
                return "Importe a reintegrar";
            }

            return "Sin diferencia";
        }

        private static IReadOnlyCollection<T>
            NormalizarColeccion<T>(
                IReadOnlyCollection<T> elementos)
        {
            return elementos
                   ?? new List<T>()
                       .AsReadOnly();
        }
    }
}
