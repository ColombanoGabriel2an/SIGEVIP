using System.Collections.Generic;
using System.Linq;
using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteViaticoResultadoDto
    {
        public ReporteViaticoResultadoDto(
            IEnumerable<ReporteViaticoFilaDto>
                filas,
            decimal totalRegistrado,
            decimal totalVigente,
            decimal totalExcluido,
            decimal promedio,
            CategoriaGasto? categoriaMayorGasto,
            decimal totalCategoriaMayorGasto,
            IEnumerable<ReporteCategoriaGastoDto>
                gastoVigentePorCategoria)
        {
            Filas =
                (filas ??
                    Enumerable.Empty
                        <ReporteViaticoFilaDto>())
                .ToList()
                .AsReadOnly();

            TotalRegistrado = totalRegistrado;
            TotalVigente = totalVigente;
            TotalExcluido = totalExcluido;
            Promedio = promedio;
            CategoriaMayorGasto =
                categoriaMayorGasto;
            TotalCategoriaMayorGasto =
                totalCategoriaMayorGasto;

            GastoVigentePorCategoria =
                (gastoVigentePorCategoria ??
                    Enumerable.Empty
                        <ReporteCategoriaGastoDto>())
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyCollection
            <ReporteViaticoFilaDto>
            Filas
        {
            get;
            private set;
        }

        public int Cantidad
        {
            get
            {
                return Filas.Count;
            }
        }

        public decimal TotalRegistrado
        {
            get;
            private set;
        }

        public decimal TotalVigente
        {
            get;
            private set;
        }

        public decimal TotalExcluido
        {
            get;
            private set;
        }

        public decimal Promedio
        {
            get;
            private set;
        }

        public CategoriaGasto? CategoriaMayorGasto
        {
            get;
            private set;
        }

        public decimal TotalCategoriaMayorGasto
        {
            get;
            private set;
        }

        public IReadOnlyCollection
            <ReporteCategoriaGastoDto>
            GastoVigentePorCategoria
        {
            get;
            private set;
        }

        public string CategoriaMayorGastoTexto
        {
            get
            {
                return CategoriaMayorGasto.HasValue
                    ? CategoriaMayorGasto.Value
                        .ToString()
                    : "Sin datos";
            }
        }
    }
}
