using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteCategoriaGastoDto
    {
        public ReporteCategoriaGastoDto(
            CategoriaGasto categoria,
            decimal totalVigente)
        {
            Categoria = categoria;
            TotalVigente = totalVigente;
        }

        public CategoriaGasto Categoria
        {
            get;
            private set;
        }

        public decimal TotalVigente
        {
            get;
            private set;
        }
    }
}
