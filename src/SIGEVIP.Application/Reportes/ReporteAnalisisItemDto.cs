namespace SIGEVIP.Application.Reportes
{
    public sealed class ReporteAnalisisItemDto
    {
        public ReporteAnalisisItemDto(
            string etiqueta,
            decimal valor,
            decimal participacion = 0m)
        {
            Etiqueta =
                string.IsNullOrWhiteSpace(
                    etiqueta)
                    ? "Sin especificar"
                    : etiqueta.Trim();

            Valor = valor;
            Participacion = participacion;
        }

        public string Etiqueta
        {
            get;
            private set;
        }

        public decimal Valor
        {
            get;
            private set;
        }

        public decimal Participacion
        {
            get;
            private set;
        }
    }
}
