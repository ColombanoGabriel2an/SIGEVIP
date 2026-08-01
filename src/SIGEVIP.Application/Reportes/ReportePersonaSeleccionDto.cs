namespace SIGEVIP.Application.Reportes
{
    public sealed class ReportePersonaSeleccionDto
    {
        public ReportePersonaSeleccionDto(
            int idPersona,
            string nombreCompleto)
        {
            IdPersona = idPersona;
            NombreCompleto =
                nombreCompleto ?? string.Empty;
        }

        public int IdPersona { get; private set; }

        public string NombreCompleto
        {
            get;
            private set;
        }

        public string Presentacion
        {
            get
            {
                return NombreCompleto;
            }
        }
    }
}
