using System;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Viaticos
{
    public sealed class ViaticoFiltro
    {
        public ViaticoFiltro(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            CategoriaGasto? categoria,
            EstadoViatico? estado)
        {
            if (fechaDesde.HasValue &&
                fechaHasta.HasValue &&
                fechaDesde.Value.Date >
                    fechaHasta.Value.Date)
            {
                throw new ReglaNegocioException(
                    "La fecha desde no puede ser posterior a la fecha hasta.");
            }

            if (categoria.HasValue &&
                !Enum.IsDefined(
                    typeof(CategoriaGasto),
                    categoria.Value))
            {
                throw new ReglaNegocioException(
                    "La categoría utilizada como filtro no es válida.");
            }

            if (estado.HasValue &&
                !Enum.IsDefined(
                    typeof(EstadoViatico),
                    estado.Value))
            {
                throw new ReglaNegocioException(
                    "El estado utilizado como filtro no es válido.");
            }

            FechaDesde =
                fechaDesde.HasValue
                    ? fechaDesde.Value.Date
                    : (DateTime?)null;

            FechaHasta =
                fechaHasta.HasValue
                    ? fechaHasta.Value.Date
                    : (DateTime?)null;

            Categoria =
                categoria;

            Estado =
                estado;
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

        public CategoriaGasto? Categoria
        {
            get;
            private set;
        }

        public EstadoViatico? Estado
        {
            get;
            private set;
        }

        public static ViaticoFiltro
            CrearSinFiltros()
        {
            return new ViaticoFiltro(
                null,
                null,
                null,
                null);
        }
    }
}