using System;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteAnalisisFiltro
    {
        public ReporteAnalisisFiltro(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            ReporteAreaAnalisis area,
            ReporteIndicadorAnalisis indicador,
            ReporteAgrupacionAnalisis agrupacion,
            int top)
        {
            FechaDesde =
                fechaDesde.HasValue
                    ? fechaDesde.Value.Date
                    : (DateTime?)null;

            FechaHasta =
                fechaHasta.HasValue
                    ? fechaHasta.Value.Date
                    : (DateTime?)null;

            if (FechaDesde.HasValue &&
                FechaHasta.HasValue &&
                FechaDesde.Value >
                    FechaHasta.Value)
            {
                throw new ReglaNegocioException(
                    "La fecha desde no puede ser posterior a la fecha hasta.");
            }

            ExigirEnumDefinido(
                area,
                nameof(area));

            ExigirEnumDefinido(
                indicador,
                nameof(indicador));

            ExigirEnumDefinido(
                agrupacion,
                nameof(agrupacion));

            if (top != 0 &&
                top != 5 &&
                top != 10 &&
                top != 15)
            {
                throw new ReglaNegocioException(
                    "La cantidad máxima debe ser 5, 10, 15 o Todos.");
            }

            ValidarCombinacion(
                area,
                indicador,
                agrupacion);

            Area = area;
            Indicador = indicador;
            Agrupacion = agrupacion;
            Top = top;
        }

        public DateTime? FechaDesde
        {
            get;
            private set;
        }

        public DateTime? FechaHasta
        {
            get;
            private set;
        }

        public ReporteAreaAnalisis Area
        {
            get;
            private set;
        }

        public ReporteIndicadorAnalisis Indicador
        {
            get;
            private set;
        }

        public ReporteAgrupacionAnalisis Agrupacion
        {
            get;
            private set;
        }

        public int Top
        {
            get;
            private set;
        }

        private static void ValidarCombinacion(
            ReporteAreaAnalisis area,
            ReporteIndicadorAnalisis indicador,
            ReporteAgrupacionAnalisis agrupacion)
        {
            bool valida;

            switch (area)
            {
                case ReporteAreaAnalisis.Gastos:
                    valida =
                        indicador ==
                            ReporteIndicadorAnalisis.Importe
                        &&
                        (
                            agrupacion ==
                                ReporteAgrupacionAnalisis.Categoria
                            ||
                            agrupacion ==
                                ReporteAgrupacionAnalisis.Empleado
                            ||
                            agrupacion ==
                                ReporteAgrupacionAnalisis.Viaje
                            ||
                            agrupacion ==
                                ReporteAgrupacionAnalisis.Mes
                        );
                    break;

                case ReporteAreaAnalisis.Clientes:
                    valida =
                        indicador ==
                            ReporteIndicadorAnalisis.CantidadVisitas
                        &&
                        (
                            agrupacion ==
                                ReporteAgrupacionAnalisis.Cliente
                            ||
                            agrupacion ==
                                ReporteAgrupacionAnalisis.Localidad
                            ||
                            agrupacion ==
                                ReporteAgrupacionAnalisis.Provincia
                            ||
                            agrupacion ==
                                ReporteAgrupacionAnalisis.Mes
                        );
                    break;

                case ReporteAreaAnalisis.Viajes:
                    valida =
                        indicador ==
                            ReporteIndicadorAnalisis.CantidadViajes
                        &&
                        (
                            agrupacion ==
                                ReporteAgrupacionAnalisis.Empleado
                            ||
                            agrupacion ==
                                ReporteAgrupacionAnalisis.TipoViaje
                            ||
                            agrupacion ==
                                ReporteAgrupacionAnalisis.EstadoViaje
                            ||
                            agrupacion ==
                                ReporteAgrupacionAnalisis.Mes
                        );
                    break;

                case ReporteAreaAnalisis.Empleados:
                    valida =
                        agrupacion ==
                            ReporteAgrupacionAnalisis.Empleado
                        &&
                        (
                            indicador ==
                                ReporteIndicadorAnalisis.Importe
                            ||
                            indicador ==
                                ReporteIndicadorAnalisis.CantidadVisitas
                            ||
                            indicador ==
                                ReporteIndicadorAnalisis.CantidadViajes
                        );
                    break;

                default:
                    valida = false;
                    break;
            }

            if (!valida)
            {
                throw new ReglaNegocioException(
                    "El indicador y la agrupación no son válidos para el área seleccionada.");
            }
        }

        private static void ExigirEnumDefinido<TEnum>(
            TEnum valor,
            string nombre)
            where TEnum : struct
        {
            if (!Enum.IsDefined(
                typeof(TEnum),
                valor))
            {
                throw new ReglaNegocioException(
                    "El valor de " +
                    nombre +
                    " no es válido.");
            }
        }
    }
}
