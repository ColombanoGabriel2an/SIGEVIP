using System.Collections.Generic;

namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteAnalisisResultadoDto
    {
        public ReporteAnalisisResultadoDto(
            string titulo,
            string unidad,
            IReadOnlyCollection
                <ReporteAnalisisItemDto>
                items,
            decimal total,
            decimal maximo,
            decimal promedio,
            string elementoDestacado,
            decimal valorDestacado)
        {
            Titulo =
                titulo ?? string.Empty;

            Unidad =
                unidad ?? string.Empty;

            Items =
                items
                ?? new List
                    <ReporteAnalisisItemDto>()
                    .AsReadOnly();

            Total = total;
            Maximo = maximo;
            Promedio = promedio;

            ElementoDestacado =
                elementoDestacado
                ?? string.Empty;

            ValorDestacado =
                valorDestacado;
        }

        public string Titulo
        {
            get;
            private set;
        }

        public string Unidad
        {
            get;
            private set;
        }

        public IReadOnlyCollection
            <ReporteAnalisisItemDto>
            Items
        {
            get;
            private set;
        }

        public decimal Total
        {
            get;
            private set;
        }

        public decimal Maximo
        {
            get;
            private set;
        }

        public decimal Promedio
        {
            get;
            private set;
        }

        public string ElementoDestacado
        {
            get;
            private set;
        }

        public decimal ValorDestacado
        {
            get;
            private set;
        }
    }
}
