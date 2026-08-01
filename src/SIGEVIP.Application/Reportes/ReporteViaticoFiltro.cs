using System;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteViaticoFiltro
    {
        public ReporteViaticoFiltro(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idViaje,
            int? idPersonaPagadora,
            CategoriaGasto? categoria,
            MetodoPago? metodoPago,
            EstadoViatico? estadoViatico,
            EstadoViaje? estadoViaje)
        {
            DateTime? desdeNormalizada =
                fechaDesde.HasValue
                    ? fechaDesde.Value.Date
                    : (DateTime?)null;

            DateTime? hastaNormalizada =
                fechaHasta.HasValue
                    ? fechaHasta.Value.Date
                    : (DateTime?)null;

            if (desdeNormalizada.HasValue &&
                hastaNormalizada.HasValue &&
                desdeNormalizada.Value >
                    hastaNormalizada.Value)
            {
                throw new ReglaNegocioException(
                    "La fecha desde no puede ser posterior a la fecha hasta.");
            }

            ValidarIdOpcional(
                idViaje,
                nameof(idViaje),
                "El identificador del Viaje debe ser mayor que cero.");

            ValidarIdOpcional(
                idPersonaPagadora,
                nameof(idPersonaPagadora),
                "El identificador de la Persona pagadora debe ser mayor que cero.");

            ValidarEnumOpcional(
                categoria,
                "La categoría utilizada como filtro no es válida.");

            ValidarEnumOpcional(
                metodoPago,
                "El método de pago utilizado como filtro no es válido.");

            ValidarEnumOpcional(
                estadoViatico,
                "El estado del Viático utilizado como filtro no es válido.");

            ValidarEnumOpcional(
                estadoViaje,
                "El estado del Viaje utilizado como filtro no es válido.");

            FechaDesde = desdeNormalizada;
            FechaHasta = hastaNormalizada;
            IdViaje = idViaje;
            IdPersonaPagadora =
                idPersonaPagadora;
            Categoria = categoria;
            MetodoPago = metodoPago;
            EstadoViatico = estadoViatico;
            EstadoViaje = estadoViaje;
        }

        public DateTime? FechaDesde { get; private set; }
        public DateTime? FechaHasta { get; private set; }
        public int? IdViaje { get; private set; }
        public int? IdPersonaPagadora { get; private set; }
        public CategoriaGasto? Categoria { get; private set; }
        public MetodoPago? MetodoPago { get; private set; }
        public EstadoViatico? EstadoViatico { get; private set; }
        public EstadoViaje? EstadoViaje { get; private set; }

        public static ReporteViaticoFiltro
            CrearSinFiltros()
        {
            return new ReporteViaticoFiltro(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        private static void ValidarIdOpcional(
            int? id,
            string nombreParametro,
            string mensaje)
        {
            if (id.HasValue &&
                id.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nombreParametro,
                    mensaje);
            }
        }

        private static void ValidarEnumOpcional<TEnum>(
            TEnum? valor,
            string mensaje)
            where TEnum : struct
        {
            if (valor.HasValue &&
                !Enum.IsDefined(
                    typeof(TEnum),
                    valor.Value))
            {
                throw new ReglaNegocioException(
                    mensaje);
            }
        }
    }
}
